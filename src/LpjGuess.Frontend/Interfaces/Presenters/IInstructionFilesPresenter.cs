using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which controls a collection of instruction file views.
/// </summary>
public interface IInstructionFilesPresenter : IPresenter
{
    /// <summary>
    /// Populate the view with the given instruction files.
    /// </summary>
    void Refresh();

    /// <summary>
    /// Save any pending changes to the instruction files.
    /// </summary>
    void SaveChanges();

    /// <summary>
    /// Called when the user wants to add an instruction file to the workspace.
    /// The event parameter is the path to the instruction file to be added.
    /// </summary>
    Event<string> OnAddInsFile { get; }

    /// <summary>
    /// Called when the user wants to remove an instruction file from the
    /// workspace. The event parameter is the path to the instruction file to
    /// be removed.
    /// </summary>
    Event<string> OnRemoveInsFile { get; }
}
