namespace LpjGuess.Runner.Models;

/// <summary>
/// Progress event arguments.
/// </summary>
public class ProgressEventArgs : EventArgs
{
    /// <summary>
    /// Current progress percentage (0-100).
    /// </summary>
    public int Percentage { get; private init; }

	/// <summary>
	/// Job name.
	/// </summary>
	public Job Source { get; private init; }

    /// <summary>
    /// Create a new <see cref="ProgressEventArgs"/> instance.
    /// </summary>
    /// <param name="percentage">Job progress as a percentage.</param>
    /// <param name="job">The job which sent this progress update.</param>
    public ProgressEventArgs(int percentage, Job job)
    {
        Percentage = percentage;
        Source = job;
    }
}
