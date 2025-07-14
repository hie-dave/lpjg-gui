using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Enumerations;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface for a view which may display an instruction file to the user.
/// </summary>
public interface IWorkspaceView : IView
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
	/// Show a progress message from the user.
	/// </summary>
	/// <param name="progress">Current job progress as fraction (0-1).</param>
	void ShowProgress(double progress);

	/// <summary>
	/// Select the specified tab.
	/// </summary>
	/// <param name="tab">The tab to be selected.</param>
	void SelectTab(FileTab tab);

	/// <summary>
	/// Called when the user wants to run with a specific runner. The event
	/// parameter is the name of the runner to be used, or null if the default
	/// runner is to be used.
	/// </summary>
	Event<string?> OnRun { get; }

	/// <summary>
	/// Called when the user wants to cancel a running simulation.
	/// </summary>
	Event OnStop { get; }

	/// <summary>
	/// Called when the user wants to add a new run method.
	/// </summary>
	Event OnAddRunOption { get; }
}
