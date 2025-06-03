using LpjGuess.Frontend.Interfaces.Presenters;

namespace LpjGuess.Frontend.Events;

/// <summary>
/// Encapsulates a change to a file.
/// </summary>
public class FileChangedArgs : EventArgs
{
    /// <summary>
    /// The file that has changed.
    /// </summary>
    public string File { get; }

    /// <summary>
    /// The contents of the file.
    /// </summary>
    public string Contents { get; }

    /// <summary>
    /// The presenter for the file.
    /// </summary>
    public IInstructionFilePresenter Presenter { get; private init; }

    /// <summary>
    /// Create a new <see cref="FileChangedArgs"/> instance.
    /// </summary>
    /// <param name="file">The file that has changed.</param>
    /// <param name="contents">The contents of the file.</param>
    /// <param name="presenter">The presenter for the file.</param>
    public FileChangedArgs(string file, string contents, IInstructionFilePresenter presenter)
    {
        File = file;
        Contents = contents;
        Presenter = presenter;
    }
}
