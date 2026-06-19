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

    /// <summary>
    /// Convert a path relative to the output directory to an absolute path.
    /// </summary>
    /// <param name="relative">The relative path.</param>
    /// <returns>The absolute path.</returns>
    string GetAbsolutePath(string relative);

    /// <summary>
    /// Convert the specified path to a relative path, relative to the output
    /// directory.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The path relative to the output directory.</returns>
    string GetRelativePath(string path);
}
