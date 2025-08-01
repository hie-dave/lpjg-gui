using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Interface to a service for generating jobs to be run.
/// </summary>
public interface ISimulationService
{
    /// <summary>
    /// Generate all jobs for these instructions.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    IEnumerable<Job> GenerateAllJobs(CancellationToken ct);
}
