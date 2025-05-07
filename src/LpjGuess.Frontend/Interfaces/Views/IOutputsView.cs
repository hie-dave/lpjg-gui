using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Interface to an outputs view.
/// </summary>
public interface IOutputsView : IView
{
    /// <summary>
    /// Invoked when the user has selected an instruction file.
    /// </summary>
    Event<string> OnInstructionFileSelected { get; }

    /// <summary>
    /// Invoked when the user has selected an output file.
    /// </summary>
    Event<string> OnOutputFileSelected { get; }

    /// <summary>
    /// Populate the view with the given instruction files.
    /// </summary>
    /// <param name="instructionFiles">Instruction file paths.</param>
    void PopulateInstructionFiles(IEnumerable<string> instructionFiles);

    /// <summary>
    /// Populate the view with the given output files.
    /// </summary>
    /// <param name="outputFiles">Output file paths.</param>
    void PopulateOutputFiles(IEnumerable<string> outputFiles);
}
