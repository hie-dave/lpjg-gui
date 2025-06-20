using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Presenters.Runners;
using LpjGuess.Frontend.Views;
using LpjGuess.Frontend.Views.Dialogs;
using LpjGuess.Frontend.Views.Runners;
using LpjGuess.Runner.Models;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// Presenter for a <see cref="Configuration"/> instance.
/// </summary>
public class PreferencesPresenter : IDialogPresenter
{
	/// <summary>
	/// The preferences object.
	/// </summary>
	private readonly Configuration preferences;

	/// <summary>
	/// Action to be invoked when the dialog is closed.
	/// </summary>
	private readonly Action onClose;

	/// <summary>
	/// The view object.
	/// </summary>
	private readonly IPreferencesView view;

	/// <summary>
	/// Presenters for each of the runner objects.
	/// </summary>
	private IEnumerable<IRunnerPresenter> runnerPresenters;

	/// <summary>
	/// Create a new <see cref="PreferencesPresenter"/> instance.
	/// </summary>
	/// <param name="preferences">The preferences object.</param>
	/// <param name="onClose">Function to be called when the dialog is closed.</param>
	public PreferencesPresenter(Configuration preferences, Action onClose)
	{
		this.preferences = preferences;
		this.onClose = onClose;
		runnerPresenters = GetRunnerPresenters().ToList();
		view = new PreferencesView(preferences.PreferDarkMode, preferences.GoToLogsTabOnRun, runnerPresenters.Select(p => p.GetMetadata()).ToList());
		view.DarkModeChanged.ConnectTo(OnToggleDarkMode);
		view.GoToLogsTabChanged.ConnectTo(OnToggleGoToLogs);
		view.OnAddRunner.ConnectTo(OnAddRunner);
		view.OnDeleteRunner.ConnectTo(OnDeleteRunner);
		view.OnEditRunner.ConnectTo(OnEditRunner);
		view.OnToggleDefaultRunner.ConnectTo(OnToggleDefaultRunner);
		view.OnClose.ConnectTo(onClose);
	}

	private IEnumerable<IRunnerPresenter> GetRunnerPresenters()
	{
		foreach (IRunnerConfiguration runner in preferences.Runners)
			yield return GetPresenter(runner);
	}

	private IRunnerPresenter GetPresenter(IRunnerConfiguration runner)
	{
		bool isDefault = Configuration.Instance.GetDefaultRunner() == runner;
		if (runner is LocalRunnerConfiguration local)
			return new LocalRunnerConfigurationPresenter(local, isDefault);
		throw new InvalidOperationException($"Unknown runner configuration type: {runner.GetType().Name}");
	}

	/// <inheritdoc />
	public void Show()
	{
		view.Show();
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		view.Dispose();
	}

	/// <summary>
	/// Get the i-th runner.
	/// </summary>
	/// <param name="i">Index of the runner.</param>
	private IRunnerConfiguration GetRunner(int i)
	{
		return Configuration.Instance.Runners.ElementAt(i);
	}

	/// <summary>
	/// Update the runner config displayed in the view.
	/// </summary>
	private void UpdateRunners()
	{
		IEnumerable<IRunnerPresenter> newPresenters = GetRunnerPresenters().ToList();
		foreach (IRunnerPresenter presenter in runnerPresenters)
			presenter.Dispose();
		runnerPresenters = newPresenters;
		view.PopulateRunners(runnerPresenters.Select(p => p.GetMetadata()).ToList());
	}

	/// <summary>
	/// Set the IsDefault status of the i-th runner.
	/// </summary>
	/// <param name="i">Index of the runner.</param>
	/// <param name="isDefault">Is this runner now the default?</param>
	private void SetDefaultRunner(int i, bool isDefault)
	{
		if (!isDefault)
		{
			Configuration.Instance.DefaultRunnerIndex = -1;
			return;
		}

		if (i < 0 || i >= Configuration.Instance.Runners.Count)
		{
			Console.WriteLine($"WARNING: attempted to set default runner to{(isDefault ? "" : "not ")} {i}");
			return;
		}

		if (!isDefault)
			Configuration.Instance.DefaultRunnerIndex = -1;

		Configuration.Instance.DefaultRunnerIndex = i;
	}

	/// <summary>
	/// Called when the user has toggled the 'prefer dark mode' option.
	/// </summary>
	/// <param name="preferDarkMode">The new value of the property.</param>
	private void OnToggleDarkMode(bool preferDarkMode)
	{
		preferences.PreferDarkMode = preferDarkMode;
		// todo: prompt for restart program.
	}

	/// <summary>
	/// Called when the user has toggled the 'go to logs' option.
	/// </summary>
	/// <param name="goToLogs">The new value of the property.</param>
	private void OnToggleGoToLogs(bool goToLogs)
	{
		preferences.GoToLogsTabOnRun = goToLogs;
	}

	/// <summary>
	/// Called when the user wants to toggle the IsDefault option of the runner
	/// with the specified index.
	/// </summary>
	/// <param name="i">Index of the runner.</param>
	private void OnToggleDefaultRunner(int i)
	{
		bool oldValue = Configuration.Instance.DefaultRunnerIndex == i;
		SetDefaultRunner(i, !oldValue);
		UpdateRunners();
	}

	/// <summary>
	/// Called when the user wants to edit the runner with the specified index.
	/// </summary>
	/// <param name="i">Index of the runner.</param>
	private void OnEditRunner(int i)
	{
		IRunnerPresenter presenter = runnerPresenters.ElementAt(i);
		RunnerConfigurationView view = new RunnerConfigurationView(presenter.GetMetadata(), presenter.CreateView());
		IRunnerConfiguration runner = GetRunner(i);
		view.OnChangeName.ConnectTo(name => runner.Name = name);
		view.OnToggleDefault.ConnectTo(isDefault => SetDefaultRunner(i, isDefault));
		view.OnClose.ConnectTo(UpdateRunners);
		view.OnClose.ConnectTo(() => view.Dispose());
		view.Show();
	}

	/// <summary>
	/// Called when the user wants to delete the runner with the specified index.
	/// </summary>
	/// <param name="i">Index of the runner.</param>
	private void OnDeleteRunner(int i)
	{
		Configuration.Instance.Runners.RemoveAt(i);
		UpdateRunners();
	}

	/// <summary>
	/// Called when the user wants to add a new runner.
	/// </summary>
	private void OnAddRunner()
	{
		IEnumerable<NameAndDescription> runnerTypes = GetKnownRunnerTypes();
		string prompt = "Select a runner type";
		AskUserDialog dialog = new AskUserDialog(prompt, "Select", runnerTypes);
		dialog.OnSelected.ConnectTo(OnRunnerAdded);
		dialog.Run();
	}

	/// <summary>
	/// Called when the user has added a new runner.
	/// </summary>
	/// <param name="name">The new runner name.</param>
	private void OnRunnerAdded(string name)
	{
		IRunnerConfiguration runner = CreateRunnerConfiguration(name);
		Configuration.Instance.Runners.Add(runner);
		UpdateRunners();
		OnEditRunner(Configuration.Instance.Runners.Count - 1);
	}

	/// <summary>
	/// Enumerate the names of all known runners.
	/// </summary>
	private static IEnumerable<NameAndDescription> GetKnownRunnerTypes()
	{
		yield return new NameAndDescription("Local", "Run simulations on this machine");
	}

	/// <summary>
	/// Create a runner configuration object from a runner type name.
	/// </summary>
	/// <param name="name">Name of the runner configuration type.</param>
	private IRunnerConfiguration CreateRunnerConfiguration(string name)
	{
		// fixme
		if (name == "Local")
			return new LocalRunnerConfiguration("", "New Local Runner");
		throw new InvalidOperationException($"Unknown runner type: {name}");
	}
}
