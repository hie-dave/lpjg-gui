namespace LpjGuess.Runner.Models;

/// <summary>
/// Output helper which ignores output coming from the model.
/// </summary>
public class OutputIgnorer : IOutputHelper
{
    /// <inheritdoc />
    public void ReportError(string jobName, string output)
    {
        // Do nothing.
    }

    /// <inheritdoc />
    public void ReportOutput(string jobName, string output)
    {
        // Do nothing.
    }
}
