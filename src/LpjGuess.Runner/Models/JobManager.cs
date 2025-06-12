namespace LpjGuess.Runner.Models;

/// <summary>
/// Manages the execution of LPJ-Guess jobs.
/// </summary>
public class JobManager
{
	/// <summary>
	/// Configuration parameters for the run.
	/// </summary>
	private readonly JobManagerConfiguration settings;

	/// <summary>
	/// Progress reporter helper.
	/// </summary>
	private readonly IProgressReporter progressReporter;

	/// <summary>
	/// Helper object which handles standard output/errror from the model.
	/// </summary>
	private readonly IOutputHelper outputHandler;

	/// <summary>
	/// The jobs managed by this job manager instance.
	/// </summary>
	private readonly IReadOnlyList<Job> jobs;

	/// <summary>
	/// Dictionary mapping job names, to those jobs' progres percentages.
	/// </summary>
	private readonly IDictionary<string, int> jobProgress;

	/// <summary>
	/// Start time of the job manager.
	/// </summary>
	private readonly DateTime startTime;

	/// <summary>
	/// The last time that a progress message was written.
	/// </summary>
	private DateTime lastUpdate;

	/// <summary>
	/// Create a new <see cref="JobManager"/> instance.
	/// </summary>
	/// <param name="settings">Run settings.</param>
	/// <param name="progressReporter">Progress reporter helper.</param>
	/// <param name="outputHandler">Output handler helper.</param>
	/// <param name="jobs">Jobs to be run.</param>
	public JobManager(
		JobManagerConfiguration settings,
		IProgressReporter progressReporter,
		IOutputHelper outputHandler,
		IEnumerable<Job> jobs)
	{
		this.jobs = jobs.ToList();
		this.settings = settings;
		this.progressReporter = progressReporter;
		this.outputHandler = outputHandler;

		jobProgress = new Dictionary<string, int>();
		startTime = DateTime.Now;
		lastUpdate = DateTime.MinValue;

        if (settings.RunConfig is LocalRunnerConfiguration && Environment.ProcessorCount > 64 && settings.CpuCount > 64)
            throw new NotImplementedException("TODO: use platform-specific API to suppost >64 CPUs");
        if (settings.CpuCount > Environment.ProcessorCount)
            throw new NotImplementedException($"cpu_count must be < NCPUs ({Environment.ProcessorCount} in this case), but is: {settings.CpuCount}");
	}

	/// <summary>
	/// Run all of the jobs.
	/// </summary>
	/// <param name="jobs">Jobs to be run.</param>
	/// <param name="ct">Cancellation token.</param>
	public async Task RunAllAsync(CancellationToken ct)
	{
		if (settings.DryRun)
		{
			Console.WriteLine("Dry run - jobs would be executed");
			return;
		}

		// Set progress to 0 for all jobs. If we don't do this, only those jobs
		// which have run or are running will exist in jobProgress.
		foreach (var job in jobs)
			jobProgress[job.Name] = 0;

		await Parallel.ForEachAsync(
			jobs,
			new ParallelOptions
			{
				MaxDegreeOfParallelism = settings.CpuCount,
				CancellationToken = ct
			},
			RunJobAsync);

		// Clear progress line when done.
		WriteProgress();
	}

	/// <summary>
	/// Run a job asynchronously.
	/// </summary>
	/// <param name="job">The job to run.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	private async ValueTask RunJobAsync(Job job, CancellationToken cancellationToken)
	{
		IRunner runner = CreateRunner();
		if (runner is IMonitorableRunner monitorable)
		{
			monitorable.ProgressChanged += HandleProgress;
			monitorable.OutputReceived += HandleStdout;
			monitorable.ErrorReceived += HandleStderr;
		}
		try
		{
			await runner.RunAsync(job, cancellationToken);
		}
		finally
		{
			if (runner is IMonitorableRunner monitor)
			{
				monitor.ProgressChanged -= HandleProgress;
				monitor.OutputReceived -= HandleStdout;
				monitor.ErrorReceived -= HandleStderr;
			}
		}
	}

	/// <summary>
	/// Handle a standard error message written by a running job.
	/// </summary>
	/// <param name="sender">Sender object (a runner instance in practice).</param>
	/// <param name="args">Event data.</param>
    private void HandleStderr(object? sender, OutputEventArgs args)
    {
        outputHandler.ReportError(args.JobName, args.Data);
    }

	/// <summary>
	/// Handle a standard output message written by a running job.
	/// </summary>
	/// <param name="sender">Sender object (a runner instance in practice).</param>
	/// <param name="args">Event data.</param>
    private void HandleStdout(object? sender, OutputEventArgs args)
    {
        outputHandler.ReportOutput(args.JobName, args.Data);
    }

    /// <summary>
    /// Handle a progress message written by a job and intercepted by a job
    /// runner.
    /// </summary>
    /// <param name="sender">A job runner instance.</param>
    /// <param name="e">The event data.</param>
    private void HandleProgress(object? sender, ProgressEventArgs e)
	{
		lock (jobProgress)
		{
			jobProgress[e.JobName] = e.Percentage;

			// Only update if at least 1 second passed
			if ((DateTime.Now - lastUpdate).TotalSeconds >= 1)
			{
				lastUpdate = DateTime.Now;
				WriteProgress();
			}
		}
	}

	/// <summary>
	/// Write a message containing the current progress toward completion.
	/// </summary>
	private void WriteProgress()
	{
		// Calculate aggregate progress.
		int total = jobProgress.Values.Sum();
		double percent = jobProgress.Count > 0 ? 1.0 * total / jobProgress.Count : 0;

		// Get number of finished jobs and elapsed walltime.
		int ncomplete = jobProgress.Values.Count(c => c == 100);
		TimeSpan elapsed = DateTime.Now - startTime;

		progressReporter.ReportProgress(percent, elapsed, ncomplete, jobs.Count);
	}

	/// <summary>
	/// Create a runner for the job.
	/// </summary>
	private IRunner CreateRunner()
	{
		return settings.RunConfig.CreateRunner(settings.InputModule);
	}
}
