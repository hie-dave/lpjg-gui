namespace LpjGuess.Runner.Models;

public interface IProgressReporter
{
    /// <summary>
    /// Handle a progress update.
    /// </summary>
    /// <param name="percent">Current progress as percentage (0-100).</param>
    /// <param name="elapsed">Total elapsed walltime since start of job.</param>
    /// <param name="ncomplete">Number of finished jobs.</param>
    /// <param name="njob">Total number of jobs.</param>
    void ReportProgress(
        double percent,
        TimeSpan elapsed,
        int ncomplete,
        int njob);
}
