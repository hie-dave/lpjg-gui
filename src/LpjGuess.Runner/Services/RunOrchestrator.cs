using LpjGuess.Core.Models;
using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Services;

/// <summary>
/// This class owns the lifecycle of a simulation(s) run.
/// </summary>
/// <remarks>
/// Given a resolved simulation run plan, this class:
/// 
/// - Prepares output state
/// - Generates simulation artefacts
/// - Persists result catalog
/// - Executes jobs
/// - Returns a run result
/// </remarks>
public sealed class RunOrchestrator
{
    private readonly ExistingOutputService cleanupService;

    /// <summary>
    /// Create a new <see cref="RunOrchestrator"/> instance.
    /// </summary>
    /// <param name="cleanupService">The service to use for cleaning up existing output.</param>
    public RunOrchestrator(ExistingOutputService cleanupService)
    {
        this.cleanupService = cleanupService;
    }

    /// <summary>
    /// Execute the specified simulation plan, and return a summary of results.
    /// </summary>
    /// <param name="plan">The simulation plan to execute.</param>
    /// <param name="cleanupPolicy">Policy for handling existing output.</param>
    /// <param name="progressReporter">Reporter for progress updates.</param>
    /// <param name="outputHelper">Helper for output operations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A summary of the experiment results.</returns>
    public async Task<ExperimentResult> RunAsync(
        RunPlan plan,
        ExistingOutputPolicy cleanupPolicy,
        IProgressReporter progressReporter,
        IOutputHelper outputHelper,
        CancellationToken ct)
    {
        foreach (SimulationBatch batch in plan.Batches)
            CleanBatch(batch, cleanupPolicy);

        List<Job> jobs = new();
        foreach (SimulationBatch batch in plan.Batches)
        {
            SimulationService generator = new SimulationService(batch);
            jobs.AddRange(generator.GenerateAllJobs(ct));
        }

        JobManager jobManager = new JobManager(plan.JobManagerConfig,
                                               progressReporter, outputHelper,
                                               jobs);

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
            // TODO: improve JobManager API to expose per-job results. For now,
            // assume a single failure.
            nfail = 1;
            nsuccess = Math.Max(0, jobs.Count - nfail);
        }

        return new ExperimentResult(jobs.Count, nsuccess, nfail,
                                          jobs.Select(j => new JobResult(
            j.Name,
            jobManager.GetJobDuration(j))), exception?.Message);
    }

    private void CleanBatch(SimulationBatch batch, ExistingOutputPolicy cleanupPolicy)
    {
        cleanupService.Apply(batch, cleanupPolicy);
    }
}
