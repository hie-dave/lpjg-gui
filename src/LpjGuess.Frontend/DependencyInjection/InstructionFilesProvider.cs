using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// Encapsulates the instruction files in a workspace.
/// </summary>
public class InstructionFilesProvider : IInstructionFilesProvider
{
    /// <summary>
    /// The instruction files in the workspace.
    /// </summary>
    private List<string> instructionFiles;

    /// <summary>
    /// Event raised when the instruction files change.
    /// </summary>
    public Event<IEnumerable<string>> OnInstructionFilesChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="InstructionFilesProvider"/> instance.
    /// </summary>
    public InstructionFilesProvider() : this([])
    {
    }

    /// <summary>
    /// Create a new <see cref="InstructionFilesProvider"/> instance.
    /// </summary>
    /// <param name="instructionFiles">The instruction files.</param>
    public InstructionFilesProvider(IEnumerable<string> instructionFiles)
    {
        OnInstructionFilesChanged = new Event<IEnumerable<string>>();
        this.instructionFiles = instructionFiles.ToList();
    }

    /// <summary>
    /// Get the instruction files in the workspace.
    /// </summary>
    /// <returns>The instruction files.</returns>
    public IEnumerable<string> GetInstructionFiles() => instructionFiles;

    /// <summary>
    /// Update the instruction files.
    /// </summary>
    /// <param name="instructionFiles">The instruction files.</param>
    public void UpdateInstructionFiles(IEnumerable<string> instructionFiles)
    {
        this.instructionFiles = instructionFiles.ToList();
        OnInstructionFilesChanged.Invoke(instructionFiles);
    }
}
