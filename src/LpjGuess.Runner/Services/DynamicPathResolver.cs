using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Resolves paths for generated simulations, using a dynamic naming strategy.
/// The instruction file name and job directory are both dynamic, based on the
/// total number of simulations and instruction file, to keep paths and file
/// names short.
/// </summary>
public class DynamicPathResolver : PathResolverBase
{
    /// <summary>
    /// Number of instruction files.
    /// </summary>
    private readonly int nins;

    /// <summary>
    /// Number of simulations.
    /// </summary>
    private readonly int nsimulation;

    /// <summary>
    /// Create a new <see cref="DynamicPathResolver"/> instance.
    /// </summary>
    /// <param name="outputDirectory">The base output directory.</param>
    /// <param name="namingStrategy">The naming strategy.</param>
    /// <param name="nins">Number of instruction files.</param>
    /// <param name="nsimulation">Number of simulations.</param>
    public DynamicPathResolver(
        string outputDirectory,
        ISimulationNamingStrategy namingStrategy,
        int nins,
        int nsimulation)
        : base(outputDirectory, namingStrategy)
    {
        this.nins = nins;
        this.nsimulation = nsimulation;
    }

    /// <inheritdoc/>
    protected override string GenerateJobName(string simulationName, string insName)
    {
        if (nsimulation == 1)
            // 1 simulation - disambiguate ins file.
            return insName;

        if (nins == 1)
            // 1 ins file, multiple simulations - disambiguate simulation.
            return simulationName;

        // Multiple ins files, multiple simulations - disambiguate both.
        return $"{insName}-{simulationName}";
    }

    /// <inheritdoc/>
    protected override string GetJobDirectory(string outputDirectory, string insName, string simulationName)
    {
        // Job directory needs to be deep enough to disambiguate it from other
        // jobs.
        string jobDirectory = outputDirectory;
        if (nins > 1)
            jobDirectory = Path.Combine(jobDirectory, insName);
        if (nsimulation > 1)
            jobDirectory = Path.Combine(jobDirectory, simulationName);
        return jobDirectory;
    }
}
