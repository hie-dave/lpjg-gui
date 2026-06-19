using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Resolves paths for generated simulations, using a consistent instruction
/// file name and relative path, which are both invariant on total number of
/// simulations and instruction files.
/// </summary>
public class StaticPathResolver : PathResolverBase
{
    /// <summary>
    /// Create a new <see cref="StaticPathResolver"/> instance.
    /// </summary>
    /// <param name="outputDirectory">The base output directory.</param>
    /// <param name="namingStrategy">The naming strategy.</param>
    public StaticPathResolver(string outputDirectory,
                              ISimulationNamingStrategy namingStrategy)
        : base(outputDirectory, namingStrategy)
    {
    }

    /// <inheritdoc/>
    protected override string GenerateJobName(string simulationName, string insName)
    {
        // Multiple ins files, multiple simulations - disambiguate both.
        return $"{insName}-{simulationName}";
    }

    /// <inheritdoc/>
    protected override string GetJobDirectory(string outputDirectory, string insName, string simulationName)
    {
        return Path.Combine(
            outputDirectory,
            insName,
            simulationName
        );
    }
}
