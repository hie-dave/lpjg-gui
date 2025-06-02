using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which controls a collection of instruction file views.
/// </summary>
public interface IInstructionFilesPresenter : IPresenter<IInstructionFilesView>
{
    /// <summary>
    /// Populate the view with the given instruction files.
    /// </summary>
    /// <param name="insFiles">The instruction files with which the view should be populated.</param>
    void Populate(IEnumerable<string> insFiles);

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
