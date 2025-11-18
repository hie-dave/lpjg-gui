namespace LpjGuess.Runner.Models;

/// <summary>
/// Progress reporter which ignores all progress updates.
/// </summary>
public sealed class NullProgressReporter : IProgressReporter
{
    /// <inheritdoc />
    public void ReportProgress(double percent, TimeSpan elapsed, int ncomplete, int njob)
    {
        // Do nothing.
    }
}
