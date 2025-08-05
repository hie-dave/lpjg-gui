using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// Interface for providing instruction files.
/// </summary>
public interface IInstructionFilesProvider
{
    /// <summary>
    /// Get the instruction files.
    /// </summary>
    /// <returns>The instruction files.</returns>
    IEnumerable<string> GetInstructionFiles();

    /// <summary>
    /// Get the generated instruction files.
    /// </summary>
    /// <returns>The generated instruction files.</returns>
    IEnumerable<InstructionFile> GetGeneratedInstructionFiles();

    /// <summary>
    /// Event raised when the instruction files change.
    /// </summary>
    Event<IEnumerable<string>> OnInstructionFilesChanged { get; }
}
