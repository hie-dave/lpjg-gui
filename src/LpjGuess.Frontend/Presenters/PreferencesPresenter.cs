using LpjGuess.Core.Interfaces.Runners;
using LpjGuess.Core.Runners.Configuration;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Presenters.Runners;
using LpjGuess.Frontend.Views;
using LpjGuess.Frontend.Views.Runners;

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
		view = new PreferencesView(preferences.PreferDarkMode, runnerPresenters.Select(p => p.GetMetadata()).ToList());
		view.DarkModeChanged.ConnectTo(OnToggleDarkMode);
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
		bool isDefault = Configuration.Instance.DefaultRunner == runner;
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
		IRunnerConfiguration runner = GetRunner(i);
		if (isDefault)
			Configuration.Instance.DefaultRunner = runner;
		else if (Configuration.Instance.DefaultRunner == runner)
			Configuration.Instance.DefaultRunner = null;
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
	/// Called when the user wants to toggle the IsDefault option of the runner
	/// with the specified index.
	/// </summary>
	/// <param name="i">Index of the runner.</param>
	private void OnToggleDefaultRunner(int i)
	{
		IRunnerConfiguration runner = GetRunner(i);
		bool oldValue = Configuration.Instance.DefaultRunner == runner;
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
		throw new NotImplementedException();
	}
}
