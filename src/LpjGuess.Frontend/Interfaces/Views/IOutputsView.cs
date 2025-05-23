using System.Data;
using Dave.Benchmarks.Core.Models.Importer;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Interface to a view which renders model outputs in a tabular display.
/// </summary>
public interface IOutputsView : IView
{
    /// <summary>
    /// Get the currently-selected instruction file.
    /// </summary>
    string? InstructionFile { get; }

    /// <summary>
    /// Invoked when the user has selected an instruction file.
    /// </summary>
    Event<string> OnInstructionFileSelected { get; }

    /// <summary>
    /// Invoked when the user has selected an output file.
    /// </summary>
    Event<OutputFile> OnOutputFileSelected { get; }

    /// <summary>
    /// Populate the view with the given instruction files.
    /// </summary>
    /// <param name="instructionFiles">Instruction file paths.</param>
    void PopulateInstructionFiles(IEnumerable<string> instructionFiles);

    /// <summary>
    /// Populate the view with the given output files.
    /// </summary>
    /// <param name="outputFiles">Output file paths.</param>
    void PopulateOutputFiles(IEnumerable<OutputFile> outputFiles);

    /// <summary>
    /// Populate the tabular data widget the contents of an output file.
    /// </summary>
    /// <param name="data">The data to be displayed.</param>
    void PopulateData(DataTable data);

    /// <summary>
    /// Select the specified instruction file.
    /// </summary>
    void SelectInstructionFile(string file);

    /// <summary>
    /// Select the specified output file.
    /// </summary>
    void SelectOutputFile(OutputFile file);
}
