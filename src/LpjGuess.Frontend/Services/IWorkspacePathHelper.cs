using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Runner.Services;

namespace LpjGuess.Frontend.Services;

/// <summary>
/// Helper for working with paths in a workspace.
/// </summary>
public interface IWorkspacePathHelper
{
    /// <summary>
    /// Create a path resolver for the given experiment.
    /// </summary>
    /// <param name="experiment">The experiment.</param>
    IPathResolver CreatePathResolver(Experiment experiment);

    /// <summary>
    /// Get the naming strategy for the given experiment.
    /// </summary>
    /// <param name="experiment">The experiment.</param>
    /// <returns>The naming strategy.</returns>
    ISimulationNamingStrategy GetNamingStrategy(Experiment experiment);
}
