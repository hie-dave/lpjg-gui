namespace LpjGuess.Runner.Models;

/// <summary>
/// A class which handles stdout/stderr messages from the model via a custom
/// callback function.
/// </summary>
public class CustomOutputHelper : IOutputHelper
{
    /// <summary>
    /// Standard output handler function.
    /// </summary>
    private readonly Action<string, string> outputCallback;

    /// <summary>
    /// Standard error handler function.
    /// </summary>
    private readonly Action<string, string> errorCallback;

    /// <summary>
    /// Create a new <see cref="CustomOutputHelper"/> instance.
    /// </summary>
    /// <param name="outputCallback">Standard output handler function.</param>
    /// <param name="errorCallback">Standard error handler function.</param>
    public CustomOutputHelper(
        Action<string, string> outputCallback,
        Action<string, string> errorCallback)
    {
        this.outputCallback = outputCallback;
        this.errorCallback = errorCallback;
    }

    /// <inheritdoc /> 
    public void ReportError(string jobName, string output)
    {
        errorCallback(jobName, output);
    }

    /// <inheritdoc /> 
    public void ReportOutput(string jobName, string output)
    {
        outputCallback(jobName, output);
    }
}
