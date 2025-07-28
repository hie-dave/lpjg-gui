using LpjGuess.Core.Parsers;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using LpjGuess.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace LpjGuess.Core.Services;

/// <summary>
/// Service for resolving output file types based on instruction file parameters.
/// </summary>
public class OutputFileTypeResolver : IOutputFileTypeResolver
{
    private readonly ILogger<OutputFileTypeResolver> logger;

    private ImmutableDictionary<string, string> filenamesToTypes;
    private ImmutableDictionary<string, string> filetypesToNames;

    /// <summary>
    /// Creates a new instance of the OutputFileTypeResolver.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public OutputFileTypeResolver(ILogger<OutputFileTypeResolver> logger)
    {
        this.logger = logger;
        filenamesToTypes = ImmutableDictionary<string, string>.Empty;
        filetypesToNames = ImmutableDictionary<string, string>.Empty;
    }

    /// <summary>
    /// Builds a lookup table mapping output filenames to their corresponding
    /// file types.
    /// </summary>
    public void BuildLookupTable(InstructionFileParser parser)
    {
        // Get all known output file types.
        logger.LogTrace("Building output file name lookup table");

        IEnumerable<string> knownTypes = OutputFileDefinitions.GetAllFileTypes();
        logger.LogTrace("Discovered {count} known output file types", knownTypes.Count());

        // Get the actual file names from the instruction file.
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        var reverseBuilder = ImmutableDictionary.CreateBuilder<string, string>();
        foreach (string fileType in knownTypes)
        {
            InstructionParameter? parameter = parser.GetTopLevelParameter(fileType);
            if (parameter != null)
            {
                string filename = parameter.AsString();
                builder.Add(filename, fileType);
                reverseBuilder.Add(fileType, filename);
            }
        }

        // Create the lookup table.
        filenamesToTypes = builder.ToImmutable();
        filetypesToNames = reverseBuilder.ToImmutable();
        logger.LogTrace("Discovered {count} enabled output file types", filenamesToTypes.Count);
    }

    /// <summary>
    /// Gets the file type for a given output filename.
    /// </summary>
    /// <param name="filename">The output filename to get the type for.</param>
    /// <returns>The file type if found.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the filename is not found.</exception>
    public string GetFileType(string filename)
    {
        if (filenamesToTypes.TryGetValue(filename, out string? fileType))
            return fileType;
        throw new KeyNotFoundException($"Unable to find output file type for filename: {filename} (have {filenamesToTypes.Count} registered output file types)");
    }

    /// <summary>
    /// Try to get a file type corresponding to the specified file name.
    /// </summary>
    /// <param name="filename">The file name (e.g. "file_lai").</param>
    /// <param name="filetype">The corresponding file type, if the instruction file defines one (e.g. "lai.out").</param>
    /// <returns>The file type corresponding to the file name, if one is defined.</returns>
    public bool TryGetFileType(string filename, [NotNullWhen(true)] out string? filetype)
    {
        return filenamesToTypes.TryGetValue(filename, out filetype);
    }

    /// <summary>
    /// Gets the file name for a given output file type.
    /// </summary>
    /// <param name="filetype">The output file type to get the file naem for.</param>
    /// <returns>The file name if found.</returns>
    public string GetFileName(string filetype)
    {
        if (filetypesToNames.TryGetValue(filetype, out string? filename))
            return filename;
        throw new KeyNotFoundException($"Unable to find output file name for file type: {filetype} (have {filetypesToNames.Count} registered output file types)");
    }

    /// <summary>
    /// Gets all unique file types in the lookup table.
    /// </summary>
    /// <returns>A HashSet of all unique file types.</returns>
    public HashSet<string> GetAllFileTypes()
    {
        return new HashSet<string>(filenamesToTypes.Values);
    }
}
