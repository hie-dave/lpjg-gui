using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Resolves paths for generated simulations.
/// </summary>
public class SimulationPathResolver : IPathResolver
{
    /// <summary>
    /// The configuration parameters.
    /// </summary>
    private readonly SimulationGeneratorConfig config;

    /// <summary>
    /// Create a new <see cref="SimulationPathResolver"/> instance.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public SimulationPathResolver(SimulationGeneratorConfig config)
    {
        this.config = config;
    }

    /// <inheritdoc/>
    public string GenerateTargetInsFilePath(string insFile, ISimulation simulation)
    {
		string jobName = simulation.Name;
		string insName = Path.GetFileNameWithoutExtension(insFile);

		// If there are multiple instruction files, include the name of the
		// instruction file in the job name.
		if (config.InsFiles.Count > 1)
			jobName = $"{insName}-{jobName}";

		// If there are multiple simulations, include the name of the simulation
		// in the job name, to ensure uniqueness.
		if (config.Simulations.Count > 1)
			jobName = $"{jobName}-{simulation.Name}";

		// Job directory needs to be deep enough to disambiguate it from other
		// jobs.
		string jobDirectory = config.OutputDirectory;
		if (config.InsFiles.Count > 1)
			jobDirectory = Path.Combine(jobDirectory, insName);
		if (config.Simulations.Count > 1)
			jobDirectory = Path.Combine(jobDirectory, simulation.Name);

		string targetInsFile = Path.Combine(jobDirectory, $"{jobName}.ins");
		return targetInsFile;
    }
}
