using Dave.Benchmarks.Core.Models.Importer;
using Dave.Benchmarks.Core.Services;
using LightInject;
using LpjGuess.Core.Models;
using LpjGuess.Runner.Models;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Data.Providers;

/// <summary>
/// A data provider for model output files.
/// </summary>
public class ModelOutputReader : IDataProvider<ModelOutput>
{
    /// <summary>
    /// Name of the instruction file parameter specifying the output directory.
    /// </summary>
    private const string outputDirectoryParameter = "outputdirectory";

    /// <summary>
    /// List of instruction files and associated parsers.
    /// </summary>
    private readonly IReadOnlyList<InsFile> insFiles;

    /// <summary>
    /// Create a new <see cref="ModelOutputReader"/> instance.
    /// </summary>
    /// <param name="instructionFiles">The instruction files for which data should be read.</param>
    public ModelOutputReader(IEnumerable<string> instructionFiles)
    {
        insFiles = instructionFiles.Select(f => new InsFile(f)).ToList();
    }

    /// <inheritdoc />
    public IEnumerable<SeriesData> Read(ModelOutput source)
    {
        return insFiles.Select(f => ReadForInstructionFile(source, f));
    }

    /// <summary>
    /// Read data for a single instruction file.
    /// </summary>
    /// <param name="source">The model output.</param>
    /// <param name="instructionFile">The instruction file.</param>
    /// <returns>The data read from the instruction file.</returns>
    private SeriesData ReadForInstructionFile(
        ModelOutput source,
        InsFile instructionFile)
    {
        string fileType = source.OutputFileType;
        string fileName = instructionFile.Resolver.GetFileName(fileType);

        InstructionParameter? parameter = instructionFile.Parser.GetTopLevelParameter("outputdirectory");
        if (parameter is null)
            throw new ArgumentException($"File '{file}' does not specify an output directory");

        string outputDirectory = parameter.AsString().Trim('"');
        /*
        string outputFile = instructionFile.Resolver.GetFileName;
        task.Wait();

        Quantity quantity = task.Result;
        */
    }

    /// <summary>
    /// Data class which combines an instruction file, its parser, and output
    /// file name resolver.
    /// </summary>
    private class InsFile
    {
        /// <summary>
        /// Path to the instruction file.
        /// </summary>
        public string FileName { get; private init; }

        /// <summary>
        /// Output file type resolver.
        /// </summary>
        public OutputFileTypeResolver Resolver { get; private init; }

        /// <summary>
        /// Instruction file parser.
        /// </summary>
        public InstructionFileParser Parser { get; private init; }

        /// <summary>
        /// Create a new <see cref="InsFile"/> instance.
        /// </summary>
        /// <param name="fileName">Path to the instruction file.</param>
        public InsFile(string fileName)
        {
            FileName = fileName;

            // TODO: dependency injection. Don't create an OutputParser every time.
            var factory = new LoggerFactory();
            var logger = new Logger<OutputFileTypeResolver>(factory);
            var logger2 = new Logger<InstructionFileParser>(factory);

            // TODO: proper async support.
            Parser = new InstructionFileParser(logger2);
            Parser.ParseInstructionFileAsync(FileName).Wait();

            Resolver = new OutputFileTypeResolver(logger);
            Resolver.BuildLookupTable(Parser);

            var usefulParser = Runner.Parsers.InstructionFileParser.FromFile(fileName);
        }
    }
}
