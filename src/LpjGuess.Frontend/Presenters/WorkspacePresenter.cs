using LpjGuess.Core.Extensions;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Enumerations;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views;
using LpjGuess.Runner.Models;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a view which displays an instruction file. This presenter
/// handles logic for running the file or aborting an ongoing run.
/// </summary>
public class WorkspacePresenter : PresenterBase<IWorkspaceView, Workspace>, IWorkspacePresenter
{
	/// <summary>
	/// A preferences presenter which will be displayed if no runners exist
	/// when the user clicks run.
	/// </summary>
	private IPreferencesPresenter? propertiesPresenter;

	/// <summary>
	/// The task representing the currently-running simulations.
	/// </summary>
	private Task? simulations;

	/// <summary>
	/// Presenter which manages the individual instruction files.
	/// </summary>
	private readonly IInstructionFilesPresenter insFilesPresenter;

	/// <summary>
	/// Presenter which manages the experiments.
	/// </summary>
	private readonly IExperimentsPresenter experimentsPresenter;

	/// <summary>
	/// The outputs presenter.
	/// </summary>
	private readonly IOutputsPresenter outputsPresenter;

	/// <summary>
	/// The graphs presenter.
	/// </summary>
	private readonly IGraphsPresenter graphsPresenter;

	/// <summary>
	/// The presenter factory.
	/// </summary>
	private readonly WorkspacePresenterFactory presenterFactory;

	/// <summary>
	/// The instruction files provider.
	/// </summary>
    private readonly InstructionFilesProvider insFilesProvider;

    /// <summary>
    /// Cancellation token used to cancel running simulations.
    /// </summary>
    private CancellationTokenSource cancellationTokenSource = new();

	/// <summary>
	/// Create a new <see cref="WorkspacePresenter"/> instance for the given file.
	/// </summary>
	/// <param name="workspace">The instruction file.</param>
	/// <param name="view">The view object.</param>
	/// <param name="registry">The command registry to use for command execution.</param>
	/// <param name="presenterFactory">The presenter factory.</param>
	public WorkspacePresenter(
		Workspace workspace,
		IWorkspaceView view,
		ICommandRegistry registry,
		WorkspacePresenterFactory presenterFactory) : base(view, workspace, registry)
	{
		this.presenterFactory = presenterFactory;

		insFilesProvider = presenterFactory.Initialise(workspace.InstructionFiles);

		// Construct child presenters.
		insFilesPresenter = presenterFactory.CreatePresenter<IInstructionFilesPresenter>();
		outputsPresenter = presenterFactory.CreatePresenter<IOutputsPresenter, IOutputsView, IEnumerable<string>>(workspace.InstructionFiles);
		graphsPresenter = presenterFactory.CreatePresenter<IGraphsPresenter, IGraphsView, IEnumerable<Graph>>(workspace.Graphs);
		experimentsPresenter = presenterFactory.CreatePresenter<IExperimentsPresenter, IExperimentsView, IEnumerable<Experiment>>(workspace.Experiments);

		// Populate views.
		PopulateRunners();
		insFilesPresenter.Refresh();
		outputsPresenter.Refresh();

		// Connect events.
		view.OnRun.ConnectTo(OnRun);
		view.OnStop.ConnectTo(OnStop);
		view.OnAddRunOption.ConnectTo(OnConfigureRunners);
		insFilesPresenter.OnAddInsFile.ConnectTo(OnAddInsFile);
		insFilesPresenter.OnRemoveInsFile.ConnectTo(OnRemoveInsFile);
	}

	/// <summary>
	/// Called when the user wants to add an instruction file to the workspace.
	/// </summary>
	/// <param name="file">Path to the file to be added.</param>
    private void OnAddInsFile(string file)
    {
		// TODO: we should ensure that this file is already in the workspace,
		// either directly, or indirectly (ie imported by another ins file).

		// Add the instruction file to the workspace.
        model.InstructionFiles.Add(file);

		// Save the changes to the workspace.
		model.Save();

		// Update the instruction files provider. This will propagate the change
		// to any presenters which now need to be updated.
		insFilesProvider.UpdateInstructionFiles(model.InstructionFiles);
    }

	/// <summary>
	/// Called when the user wants to remove an instruction file from the workspace.
	/// </summary>
	/// <param name="file">Path to the file to be removed.</param>
	/// <exception cref="ArgumentException">Thrown if the specified file is not found in the workspace.</exception>
	private void OnRemoveInsFile(string file)
	{
		if (!model.InstructionFiles.Contains(file))
			throw new ArgumentException($"File '{file}' not found in workspace");

		// Remove the instruction file from the workspace.
		model.InstructionFiles.Remove(file);

		// Save the changes to the workspace.
		model.Save();

		// Update the instruction files provider. This will propagate the change
		// to any presenters which now need to be updated.
		insFilesProvider.UpdateInstructionFiles(model.InstructionFiles);
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
				propertiesPresenter = presenterFactory.CreatePresenter<IPreferencesPresenter>();
				propertiesPresenter.OnClosed.ConnectTo(OnPreferencesClosed);
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
	public override void Dispose()
	{
		SaveWorkspace();

		if (IsRunning())
			cancellationTokenSource.Cancel();
		simulations = null;
		base.Dispose();
	}

	/// <summary>
	/// Save all pending changes to the workspace.
	/// </summary>
    private void SaveWorkspace()
    {
		// Save changes to instruction files.
		insFilesPresenter.SaveChanges();

		// Save changes to the file.
		model.Graphs = graphsPresenter.GetGraphs().ToList();
		model.Experiments = experimentsPresenter.GetExperiments().ToList();
		model.Save();
    }

	/// <summary>
	/// Run the file with the specified runner configuration.
	/// </summary>
	/// <param name="runConfig">A runner configuration.</param>
	private void Run(IRunnerConfiguration runConfig)
	{
		if (IsRunning())
			throw new InvalidOperationException($"Simulation is already running. Please kill the previous process first.");

		// Save any pending changes to the instruction files.
		insFilesPresenter.SaveChanges();

		// Clear output buffer from any previous runs.
		view.ClearOutput();

		if (!cancellationTokenSource.TryReset())
			cancellationTokenSource = new CancellationTokenSource();

		// var simulations = workspace.InstructionFiles.Select(i => new SimulationConfiguration(i, view.InputModule));
		IEnumerable<Job> jobs = model.InstructionFiles.Select(i => new Job(GetJobName(i), i));
		int cpuCount = Environment.ProcessorCount;
		var progress = new CustomProgressReporter(ProgressCallback);
		IOutputHelper outputHandler = new CustomOutputHelper(StdoutCallback, StderrCallback);
		JobManagerConfiguration settings = new JobManagerConfiguration(runConfig, cpuCount, false, view.InputModule);

		JobManager jobManager = new JobManager(settings, progress, outputHandler, jobs);
		simulations = jobManager.RunAllAsync(cancellationTokenSource.Token)
							    .ContinueWithOnMainThread(() => OnCompleted());

		// Ensure that the stop button is visible and the run button hidden.
		view.ShowRunButton(false);

		if (Configuration.Instance.GoToLogsTabOnRun)
			view.SelectTab(FileTab.Logs);
	}

    /// <summary>
    /// Generate a job name for the given instruction file.
    /// </summary>
    /// <param name="insFile">Path to an instruction file.</param>
    /// <returns>A suitable job name.</returns>
    private static string GetJobName(string insFile)
	{
		return Path.GetFileNameWithoutExtension(insFile);
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
			view.LogsView.AppendLine("Simulations were cancelled by the user");
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to abort execution of file '{model.InstructionFiles}'", error);
		}
	}

	/// <summary>
	/// The guess process has completed.
	/// </summary>
	private void OnCompleted()
	{
		try
		{
			view.ShowRunButton(true);
			outputsPresenter.Refresh();
			graphsPresenter.RefreshAll();
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
	/// Called whenever a guess process writes a progress message.
	/// </summary>
	/// <param name="percent">Current overall progress in percent (0-100).</param>
	/// <param name="elapsed">Total elapsed walltime since start of jobs' execution.</param>
	/// <param name="ncomplete">Number of completed jobs.</param>
	/// <param name="njob">Total number of jobs.</param>
    private void ProgressCallback(double percent, TimeSpan elapsed, int ncomplete, int njob)
    {
        MainView.RunOnMainThread(() => view.ShowProgress(percent / 100.0));
    }

	/// <summary>
	/// Called whenever a guess process writes to stdout. Propagates the message
	/// to the user.
	/// </summary>
	/// <param name="jobName">Name of the job writing to stdout.</param>
	/// <param name="stdout">The message written by guess to stdout.</param>
	private void StdoutCallback(string jobName, string stdout)
	{
		MainView.RunOnMainThread(() => view.LogsView.AppendLine(stdout));
	}

	/// <summary>
	/// Called whenever a guess process writes to stderr. Propagates the message
	/// to the user.
	/// </summary>
	/// <param name="jobName">Name of the job writing to stderr.</param>
	/// <param name="stderr">The message written by guess to stderr.</param>
	private void StderrCallback(string jobName, string stderr)
	{
		MainView.RunOnMainThread(() => view.LogsView.AppendLine(stderr));
	}
}
