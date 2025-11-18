using LpjGuess.Runner.Models;
using LpjGuess.Runner.Services;

namespace LpjGuess.Runner;

/// <summary>
/// Public API for running experiments.
/// </summary>
public sealed class ExperimentRunner
{
    /// <summary>
    /// Asynchronously runs an experiment.
    /// </summary>
    /// <param name="config">Configuration for the experiment.</param>
    /// <param name="reporter">Optional progress reporter. If null, progress is ignored.</param>
    /// <param name="helper">Optional output helper. If null, output is ignored.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="ExperimentResult"/> with summary information.</returns>
    public async Task<ExperimentResult> RunAsync(
        RunnerConfiguration config,
        IProgressReporter? reporter,
        IOutputHelper? helper,
        CancellationToken ct)
    {
        SimulationGeneratorConfig generatorConfig = new SimulationGeneratorConfig(
            config.Settings.OutputDirectory,
            config.Settings.Parallel,
            config.Settings.CpuCount,
            config.Factors,
            config.InsFiles,
            config.Pfts);
        SimulationService generator = new SimulationService(generatorConfig);
        List<Job> jobs = generator.GenerateAllJobs(ct).ToList();

        // Use provided reporter/output, or sensible no-op defaults for library
        // use.
        reporter ??= new NullProgressReporter();
        helper ??= new OutputIgnorer();

        JobManagerConfiguration jobSettings = config.Settings.ToJobManagerConfig();
        JobManager jobManager = new JobManager(jobSettings, reporter, helper, jobs);

        Exception? exception = null;
        int nsuccess;
        int nfail;
        try
        {
            await jobManager.RunAllAsync(ct);
            nsuccess = jobs.Count;
            nfail = 0;
        }
        catch (ModelException ex)
        {
            exception = ex;
            // TODO: improve JobManager API to expose per-job results. For now, assume a single failure.
            nfail = 1;
            nsuccess = Math.Max(0, jobs.Count - nfail);
        }

        var result = new ExperimentResult(jobs.Count, nsuccess, nfail, exception?.Message);
        return result;
    }

    /// <summary>
    /// Asynchronously runs an experiment with default reporter/output.
    /// </summary>
    public Task<ExperimentResult> RunAsync(RunnerConfiguration config, CancellationToken ct)
        => RunAsync(config, reporter: null, helper: null, ct);

    /// <summary>
    /// Runs an experiment synchronously.
    /// </summary>
    /// <param name="config">Configuration for the experiment.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="output">Optional output helper.</param>
    /// <returns>An <see cref="ExperimentResult"/> summarizing the run.</returns>
    public ExperimentResult Run(
        RunnerConfiguration config,
        IProgressReporter? progress = null,
        IOutputHelper? output = null)
    {
        using CancellationTokenSource cts = new CancellationTokenSource();
        return RunAsync(config, progress, output, cts.Token).GetAwaiter().GetResult();
    }
}
