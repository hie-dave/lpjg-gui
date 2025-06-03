using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which controls an instruction file view.
/// </summary>
public interface IInstructionFilePresenter : IPresenter<IInstructionFileView>
{
    /// <summary>
    /// Save all changes to the instruction file, and all recursively imported
    /// instruction files.
    /// </summary>
    void SaveChanges();

    /// <summary>
    /// Notify the presenter that the specified file has changed.
    /// </summary>
    /// <param name="args">The event data.</param>
    void NotifyFileChanged(FileChangedArgs args);

    /// <summary>
    /// Notify the presenter that the specified file has been saved.
    /// </summary>
    /// <param name="file">The file that has been saved.</param>
    void NotifyFileSaved(string file);

    /// <summary>
    /// Event raised when a file has been changed by the user.
    /// </summary>
    Event<FileChangedArgs> OnFileChanged { get; }

    /// <summary>
    /// Event raised when the changes to an instruction file have been saved.
    /// </summary>
    Event<string> OnSaved { get; }
}
