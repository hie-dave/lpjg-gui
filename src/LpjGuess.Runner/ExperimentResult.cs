namespace LpjGuess.Runner;

/// <summary>
/// Result of an experiment run.
/// </summary>
public sealed class ExperimentResult
{
    /// <summary>
    /// Total number of jobs submitted.
    /// </summary>
    public int TotalJobs { get; private init; }

    /// <summary>
    /// Number of jobs that completed successfully.
    /// </summary>
    public int SuccessfulJobs { get; private init; }

    /// <summary>
    /// Number of jobs that failed.
    /// </summary>
    public int FailedJobs { get; private init; }

    /// <summary>
    /// Error message, if any.
    /// </summary>
    public string? Error { get; private init; }

    /// <summary>
    /// Dictionary mapping job names to their durations.
    /// </summary>
    public Dictionary<string, TimeSpan> JobDurations { get; private init; }

    /// <summary>
    /// Create a new <see cref="ExperimentResult"/> instance.
    /// </summary>
    /// <param name="totalJobs">Total number of jobs submitted.</param>
    /// <param name="successfulJobs">Number of jobs that completed successfully.</param>
    /// <param name="failedJobs">Number of jobs that failed.</param>
    /// <param name="jobDurations">Dictionary mapping job names to their durations.</param>
    /// <param name="error">Error message, if any.</param>
    public ExperimentResult(int totalJobs, int successfulJobs, int failedJobs,
                            Dictionary<string, TimeSpan> jobDurations,
                            string? error)
    {
        TotalJobs = totalJobs;
        SuccessfulJobs = successfulJobs;
        FailedJobs = failedJobs;
        Error = error;
        JobDurations = jobDurations;
    }
}
