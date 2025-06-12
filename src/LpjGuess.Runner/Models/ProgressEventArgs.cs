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
	public string JobName { get; private init; }

    /// <summary>
    /// Create a new <see cref="ProgressEventArgs"/> instance.
    /// </summary>
    /// <param name="percentage">Job progress as a percentage.</param>
    /// <param name="jobName">Name of the job.</param>
    public ProgressEventArgs(int percentage, string jobName)
    {
        Percentage = percentage;
        JobName = jobName;
    }
}
