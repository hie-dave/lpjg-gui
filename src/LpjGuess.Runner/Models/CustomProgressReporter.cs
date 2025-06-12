namespace LpjGuess.Runner.Models;

/// <summary>
/// A class which allows the use of a custom/dynamic progress handling method.
/// </summary>
public class CustomProgressReporter : IProgressReporter
{
    /// <summary>
    /// User-defined callback function which handles progress reports. The
    /// parameters are: progress as percentage (0-100), elapsed time, number of
    /// complete jobs, and total number of jobs.
    /// </summary>
    private readonly Action<double, TimeSpan, int, int> callback;

    /// <summary>
    /// Create a new <see cref="CustomProgressReporter"/> instance.
    /// </summary>
    /// <param name="callback">User-defined callback function which handles
    /// progress reports. The parameters are: progress as percentage (0-100),
    /// elapsed time, number of complete jobs, and total number of jobs.</param>
    public CustomProgressReporter(Action<double, TimeSpan, int, int> callback)
    {
        this.callback = callback;
    }

    /// <inheritdoc />
    public void ReportProgress(double percent, TimeSpan elapsed, int ncomplete, int njob)
    {
        callback(percent, elapsed, ncomplete, njob);
    }
}
