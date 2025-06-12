namespace LpjGuess.Runner.Models;

/// <summary>
/// Class which handles standard output/error by streaming it to the console.
/// </summary>
public class ConsoleOutputHelper : IOutputHelper
{
    /// <inheritdoc />
    public void ReportOutput(string jobName, string output)
    {
        Console.WriteLine($"[{jobName}] {output}");
    }

    /// <inheritdoc />
    public void ReportError(string jobName, string output)
    {
        Console.Error.WriteLine($"[{jobName}] {output}");
    }
}
