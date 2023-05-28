using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface for a view which may display an instruction file to the user.
/// </summary>
public interface IFileView : IView
{
	/// <summary>
	/// Currently-selected input module.
	/// </summary>
	string InputModule { get; }

	/// <summary>
	/// Append a tab to the file view.
	/// </summary>
	/// <param name="name">Name of the tab.</param>
	/// <param name="widget">The tab widget.</param>
	void AppendTab(string name, IView widget);

	/// <summary>
	/// Clear the buffer of messages received from lpj-guess.
	/// </summary>
	void ClearOutput();

	/// <summary>
	/// Populate the runners dropdown with the given list of options. When one
	/// of them is activated by the user, <see cref="OnRun"/> will be invoked
	/// with the name of the selected runner.
	/// </summary>
	/// <param name="runners">Runner names.</param>
	void SetRunners(IEnumerable<string> runners);

	/// <summary>
	/// Toggle the visibility of the run/stop buttons.
	/// </summary>
	/// <param name="show">
	/// If true, the run button will be shown and the stop button hidden. If
	/// false, the run button will be hidden and the stop button shown.
	/// </param>
	void ShowRunButton(bool show);

	/// <summary>
	/// Get a reference to the graphs view.
	/// </summary>
	IGraphsView GraphsView { get; }

	/// <summary>
	/// A child view which displays console output from runs of the model.
	/// </summary>
	IEditorView OutputView { get; }

	/// <summary>
	/// Called when the user wants to run with a specific runner.
	/// </summary>
	Event<string> OnRun { get; }
}
