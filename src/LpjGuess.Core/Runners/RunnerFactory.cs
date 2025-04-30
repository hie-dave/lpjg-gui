using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Runners;
using LpjGuess.Core.Runners.Configuration;

namespace LpjGuess.Core.Runners;

/// <summary>
/// This class creates <see cref="IRunner"/> instances from run configurations.
/// </summary>
public static class RunnerFactory
{
	/// <summary>
	/// Create a runner instance for the given run configuration.
	/// </summary>
	/// <param name="runConfig">Configuration settings which describe how to run the model.</param>
	/// <param name="simulation">Settings which describe how to run a particular simulation.</param>
	/// <param name="stdoutCallback">Function to be called when the process writes to stdout.</param>
	/// <param name="stderrCallback">Function to be called when the process writes to stderr.</param>
	/// <param name="onCompleted">Function to be called when the process exits. The function argument is the exit code of the process.</param>
	/// <param name="onProgressChanged">Function to be called when the process reports progress. The function argument is the progress as a fraction (0-1).</param>
	public static IRunner Create(IRunnerConfiguration runConfig, ISimulation simulation,
		Action<string> stdoutCallback, Action<string> stderrCallback, Action<int> onCompleted, Action<double> onProgressChanged)
	{
		if (runConfig is LocalRunnerConfiguration local)
			return new LocalRunner(local, simulation, stdoutCallback, stderrCallback, onProgressChanged, onCompleted);

		throw new InvalidOperationException($"Unsupported runner configuration: {runConfig.GetType().Name}");
	}
}
