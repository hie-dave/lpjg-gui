using System.Linq.Expressions;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Enumerations;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Views;
using LpjGuess.Runner.Models;

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
	private readonly Workspace workspace;

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
	/// The task representing the currently-running simulations.
	/// </summary>
	private Task? simulations;

	/// <summary>
	/// The graphs presenter.
	/// </summary>
	private readonly IGraphsPresenter graphsPresenter;

	private readonly CancellationTokenSource cancellationTokenSource = new();

	/// <summary>
	/// Create a new <see cref="FilePresenter"/> instance for the given file.
	/// </summary>
	/// <param name="workspace">The instruction file.</param>
	public FilePresenter(Workspace workspace)
	{
		this.workspace = workspace;
		view = new FileView(workspace.InstructionFiles);
		view.OnRun.ConnectTo(OnRun);
		view.OnStop.ConnectTo(OnStop);
		view.OnAddRunOption.ConnectTo(OnConfigureRunners);
		this.outputView = view.LogsView;
		graphsPresenter = new GraphsPresenter(view.GraphsView, workspace.Graphs);
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
		workspace.Graphs = graphsPresenter.GetGraphs().ToList();
		workspace.Save();

		view.Dispose();
		if (IsRunning())
			cancellationTokenSource.Cancel();
		simulations = null;
	}

	/// <inheritdoc />
	public IFileView GetView() => view;

	/// <summary>
	/// Run the file with the specified runner configuration.
	/// </summary>
	/// <param name="runConfig">A runner configuration.</param>
	private void Run(IRunnerConfiguration runConfig)
	{
		if (IsRunning())
			throw new InvalidOperationException($"Simulation is already running. Please kill the previous process first.");

		// Clear output buffer from any previous runs.
		view.ClearOutput();

		// var simulations = workspace.InstructionFiles.Select(i => new SimulationConfiguration(i, view.InputModule));
		IEnumerable<Job> jobs = workspace.InstructionFiles.Select(i => new Job(Path.GetFileNameWithoutExtension(i), i));
		int cpuCount = Environment.ProcessorCount;
		var progress = new CustomProgressReporter((p, _, __, ___) => view.ShowProgress(p));
		JobManagerConfig settings = new JobManagerConfig(cpuCount, false, runConfig, view.InputModule);

		JobManager jobManager = new JobManager(settings, jobs, progress);
		simulations = jobManager.RunAllAsync(cancellationTokenSource.Token);
		// runner.OnProgressChanged.ConnectTo(OnProgressReceived);
		// runner.Run();

		// Ensure that the stop button is visible and the run button hidden.
		view.ShowRunButton(false);

		if (Configuration.Instance.GoToLogsTabOnRun)
			view.SelectTab(FileTab.Logs);
	}

	/// <summary>
	/// Called when a progress report is received from the model.
	/// </summary>
	/// <param name="progress">Current model execution progress (0-1).</param>
    private void OnProgressReceived(double progress)
    {
        view.ShowProgress(progress);
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
			if (!IsRunning())
				return;

			cancellationTokenSource.Cancel();
			view.ShowRunButton(true);
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to abort execution of file '{workspace.InstructionFiles}'", error);
		}
	}

	/// <summary>
	/// The guess process has completed.
	/// </summary>
	private void OnCompleted(int exitCode)
	{
		try
		{
			if (simulations != null && simulations.IsCompleted)
				MainView.RunOnMainThread(() => outputView.AppendLine($"Process exited with code {exitCode}"));
			view.ShowRunButton(true);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Check if any simulations are currently running.
	/// </summary>
	/// <returns>True iff any simulations are currently running.</returns>
	private bool IsRunning()
	{
		return simulations != null && !simulations.IsCompleted;
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
