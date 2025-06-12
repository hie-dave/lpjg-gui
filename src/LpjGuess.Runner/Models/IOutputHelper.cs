namespace LpjGuess.Runner.Models;

/// <summary>
/// Interface to a class which handles stdout/stderr data from the model.
/// </summary>
public interface IOutputHelper
{
    /// <summary>
    /// Report a message received from a job's stdout stream.
    /// </summary>
    /// <param name="jobName">Name of the job.</param>
    /// <param name="output">Standard output received from the job.</param>
    void ReportOutput(string jobName, string output);

    /// <summary>
    /// Report a message received from the job's stderr stream.
    /// </summary>
    /// <param name="jobName">Name of the job.</param>
    /// <param name="output">Standard error received from the job.</param>
    void ReportError(string jobName, string output);
}
