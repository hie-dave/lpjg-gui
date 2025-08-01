using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Resolves paths for generated simulations.
/// </summary>
public interface IPathResolver
{
    /// <summary>
    /// Generate a target instruction file path for the specified simulation.
    /// </summary>
    /// <param name="insFile">The instruction file.</param>
    /// <param name="simulation">The simulation.</param>
    /// <returns>The target instruction file path.</returns>
    string GenerateTargetInsFilePath(string insFile, ISimulation simulation);
}
