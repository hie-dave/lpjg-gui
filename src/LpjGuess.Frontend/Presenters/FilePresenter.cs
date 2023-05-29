using LpjGuess.Core.Interfaces.Runners;
using LpjGuess.Core.Models;
using LpjGuess.Core.Runners;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Enumerations;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a view which displays an instruction file. This presenter
/// handles logic for running the file or aborting an ongoing run.
/// </summary>
public class FilePresenter : IPresenter<IFileView>
{
	/// <summary>
	/// The current workspace metadata.
	/// </summary>
	private readonly LpjFile lpjFile;

	/// <summary>
	/// The view object.
	/// </summary>
	private readonly IFileView view;

	/// <summary>
	/// Widget containing console output from the guess process.
	/// </summary>
	private readonly IEditorView outputView;

	/// <summary>
	/// A preferences presenter which will be displayed if no runners exist
	/// when the user clicks run.
	/// </summary>
	private PreferencesPresenter? propertiesPresenter;

	/// <summary>
	/// The runner object. fixme - let's make this non-nullable and reuse it?
	/// </summary>
	private IRunner? runner;

	/// <summary>
	/// The graphs presenter.
	/// </summary>
	private readonly IGraphsPresenter graphsPresenter;

	/// <summary>
	/// Create a new <see cref="FilePresenter"/> instance for the given file.
	/// </summary>
	/// <param name="file">The instruction file.</param>
	public FilePresenter(LpjFile file)
	{
		this.lpjFile = file;
		view = new FileView(file.InstructionFile);
		view.OnRun.ConnectTo(OnRun);
		view.OnStop.ConnectTo(OnStop);
		view.OnAddRunOption.ConnectTo(OnConfigureRunners);
		this.outputView = view.LogsView;
		graphsPresenter = new GraphsPresenter(view.GraphsView, file.Graphs);
		PopulateRunners();
	}

	/// <summary>
	/// Called when the user wants to configure the available runners.
	/// </summary>
	private void OnConfigureRunners()
	{
		try
		{
			if (propertiesPresenter == null)
			{
				propertiesPresenter = new PreferencesPresenter(Configuration.Instance, OnPreferencesClosed);
				propertiesPresenter.Show();
			}
			else
				propertiesPresenter.Show();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Preferences presenter has been closed by the user.
	/// </summary>
	private void OnPreferencesClosed()
	{
		try
		{
			propertiesPresenter?.Dispose();
			propertiesPresenter = null;
			PopulateRunners();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		// Save changes to the file.
		lpjFile.Graphs = graphsPresenter.GetGraphs().ToList();
		lpjFile.Save();

		view.Dispose();
		runner?.Dispose();
	}

	/// <inheritdoc />
	public IFileView GetView() => view;

	/// <summary>
	/// Run the file with the specified runner configuration.
	/// </summary>
	/// <param name="runConfig">A runner configuration.</param>
	private void Run(IRunnerConfiguration runConfig)
	{
		if (runner != null)
		{
			if (runner.IsRunning)
				throw new InvalidOperationException($"Simulation is already running. Please kill the previous process first.");

			// Dispose of the previous runner object.
			runner.Dispose();
		}

		// Clear output buffer from any previous runs.
		view.ClearOutput();

		var simulation = new SimulationConfiguration(lpjFile.InstructionFile, view.InputModule);
		runner = RunnerFactory.Create(runConfig, simulation, StdoutCallback, StderrCallback, OnCompleted);
		runner.Run();

		// Ensure that the stop button is visible and the run button hidden.
		view.ShowRunButton(false);

		if (Configuration.Instance.GoToLogsTabOnRun)
			view.SelectTab(FileTab.Logs);
	}

	/// <summary>
	/// Populate the runners dropdown in the view.
	/// </summary>
	public void PopulateRunners()
	{
		view.SetRunners(Configuration.Instance.Runners.Select(r => r.Name));
	}

	/// <summary>
	/// Get the runner configuration with the specified name.
	/// </summary>
	/// <param name="name">Name of a runner configuration.</param>
	private IRunnerConfiguration? GetRunner(string? name)
	{
		Configuration conf = Configuration.Instance;
		if (name == null)
			return conf.GetDefaultRunner();
		return conf.Runners.FirstOrDefault(r => r.Name.Equals(name, StringComparison.CurrentCulture));
	}

	/// <summary>
	/// User wants to run the runner with the specified name.
	/// </summary>
	/// <param name="name">Name of the runner.</param>
	private void OnRun(string? name)
	{
		IRunnerConfiguration? config = GetRunner(name);
		if (config == null)
		{
			if (name == null)
				throw new InvalidOperationException("No default runner exists");
			// todo: this should probably be a warning.
			throw new InvalidOperationException($"Unknown runner configuration: '{name}'");
		}

		Run(config);
	}

	/// <summary>
	/// User has clicked the 'stop' button.
	/// </summary>
	private void OnStop()
	{
		try
		{
			if (runner == null || !runner.IsRunning)
				return;

			runner.Cancel();
			view.ShowRunButton(true);
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to abort execution of file '{lpjFile.InstructionFile}'", error);
		}
	}

	/// <summary>
	/// The guess process has completed.
	/// </summary>
	private void OnCompleted(int exitCode)
	{
		try
		{
			if (runner != null && !runner.IsRunning)
				MainView.RunOnMainThread(() => outputView.AppendLine($"Process exited with code {exitCode}"));
			view.ShowRunButton(true);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Called whenever a guess process writes to stdout. Propagates the message
	/// to the user.
	/// </summary>
	/// <param name="stdout">The message written by guess to stdout.</param>
	private void StdoutCallback(string stdout)
	{
		MainView.RunOnMainThread(() => outputView.AppendLine(stdout));
	}

	/// <summary>
	/// Called whenever a guess process writes to stderr. Propagates the message
	/// to the user.
	/// </summary>
	/// <param name="stderr">The message written by guess to stderr.</param>
	private void StderrCallback(string stderr)
	{
		MainView.RunOnMainThread(() => outputView.AppendLine(stderr));
	}
}
