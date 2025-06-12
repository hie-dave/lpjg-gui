namespace LpjGuess.Runner.Models;

/// <summary>
/// Helper class which writes progress updates to standard output.
/// </summary>
public class ConsoleProgressReporter : IProgressReporter
{
    /// <inheritdoc /> 
    public void ReportProgress(double percent, TimeSpan elapsed, int ncomplete, int njob)
    {
        double progress = percent / 100.0;

        if (progress < 1e-3)
		{
			Console.Write($"\r{percent:f2}% complete");
			return;
		}

        TimeSpan totalTime = elapsed / progress;
		TimeSpan remaining = totalTime - elapsed;

		Console.Write($"\r{percent:f2}% complete, {elapsed:hh\\:mm\\:ss} elapsed, {remaining:hh\\:mm\\:ss} remaining ({ncomplete}/{njob} simulations complete)");
    }
}
