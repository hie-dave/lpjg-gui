using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a preferences view.
/// </summary>
public interface IPreferencesView : IDialogView
{
	/// <summary>
	/// Called when the user changes the 'prefer dark mode' option.
	/// </summary>
	Event<bool> DarkModeChanged { get; }

	/// <summary>
	/// Called when the user wants to add a new runner.
	/// </summary>
	Event OnAddRunner { get; }

	/// <summary>
	/// Called when the user wants to delete the runner with the specified index.
	/// </summary>
	Event<int> OnDeleteRunner { get; }

	/// <summary>
	/// Called when the user wants to edit the runner with the specified index.
	/// </summary>
	Event<int> OnEditRunner { get; }

	/// <summary>
	/// Called when the user wants to toggle the IsDefault status of the runner
	/// with the specified index.
	/// </summary>
	Event<int> OnToggleDefaultRunner { get; }

	/// <summary>
	/// Populate the runners page with the specified metadata.
	/// </summary>
	/// <param name="runnerMetadata">The runners' metadata.</param>
	void PopulateRunners(IReadOnlyList<IRunnerMetadata> runnerMetadata);
}
