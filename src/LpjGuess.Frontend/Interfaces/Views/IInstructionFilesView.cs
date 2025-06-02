using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Interface to an instruction files view.
/// </summary>
public interface IInstructionFilesView : IView
{
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

    /// <summary>
    /// Populate the view with the given instruction files.
    /// </summary>
    /// <param name="insFileViews">The instruction files with which the view should be populated.</param>
    void Populate(IEnumerable<IInstructionFileView> insFileViews);
}
