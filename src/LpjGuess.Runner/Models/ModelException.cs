namespace LpjGuess.Runner.Models;

/// <summary>
/// Exception thrown when a model run fails.
/// </summary>
public class ModelException : Exception
{
    /// <summary>
    /// Create a new <see cref="ModelException"/> instance.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="jobName">The name of the job which failed.</param>
    /// <param name="exitCode">The exit code of the model process.</param>
    /// <param name="modelOutput">Standard output and error from the model.</param>
    public ModelException(string message, string jobName, int exitCode, string modelOutput) :
        base($"{message}\nJob: {jobName}\nExit Code: {exitCode}\n\n{modelOutput}")
    { }
}
