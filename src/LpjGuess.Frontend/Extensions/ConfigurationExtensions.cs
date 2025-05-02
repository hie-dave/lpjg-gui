using LpjGuess.Runner.Models;

namespace LpjGuess.Frontend.Extensions;

/// <summary>
/// Extension methods for the configuration class.
/// </summary>
public static class ConfigurationExtensions
{
	/// <summary>
	/// Get the default runner for the given configuration settings.
	/// </summary>
	/// <param name="instance">A configuration settings instance.</param>
	public static IRunnerConfiguration? GetDefaultRunner(this Configuration instance)
	{
		if (instance.DefaultRunnerIndex >= instance.Runners.Count || 
			instance.DefaultRunnerIndex < 0)
			return null;

		return instance.Runners[instance.DefaultRunnerIndex];
	}
}
