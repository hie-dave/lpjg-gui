using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Strategy for naming simulations.
/// </summary>
public interface ISimulationNamingStrategy
{
    /// <summary>
    /// Generate a name for the specified simulation.
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    /// <returns>The name.</returns>
    string GenerateName(ISimulation simulation);
}
