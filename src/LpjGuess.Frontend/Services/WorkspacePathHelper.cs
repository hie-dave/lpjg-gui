using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Runner.Services;

namespace LpjGuess.Frontend.Services;

/// <summary>
/// Resolves paths in a workspace.
/// </summary>
public class WorkspacePathResolver : IWorkspacePathHelper
{
    /// <summary>
    /// The workspace-level output directory.
    /// </summary>
    private string? outputDirectory;

    /// <summary>
    /// Initialise the resolver with the output directory.
    /// </summary>
    /// <param name="outputDirectory">The output directory.</param>
    public void Initialise(string outputDirectory)
    {
        this.outputDirectory = outputDirectory;
    }

    /// <inheritdoc/>
    public IPathResolver CreatePathResolver(Experiment experiment)
    {
        string outputDirectory = GetOutputDirectory(experiment);
        ISimulationNamingStrategy namingStrategy = GetNamingStrategy(experiment);
        return new StaticPathResolver(outputDirectory, namingStrategy);
    }

    /// <inheritdoc/>
    public ISimulationNamingStrategy GetNamingStrategy(Experiment experiment)
    {
        // TODO: make this configurable.
        return new ManualNamingStrategy();
    }

    /// <inheritdoc/>
    public IEnumerable<SimulationManifest> GetGeneratedSimulations()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    private string GetOutputDirectory(Experiment experiment)
    {
        if (outputDirectory == null)
            throw new InvalidOperationException("Output directory has not been set.");
        // TODO: sanitise experiment name?
        return Path.Combine(outputDirectory, experiment.Name);
    }
}
