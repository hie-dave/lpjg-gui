using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Runner.Services;

/// <summary>
/// A naming strategy which uses the simulation name as-is.
/// </summary>
public class ManualNamingStrategy : ISimulationNamingStrategy
{
    /// <inheritdoc/>
    public string GenerateName(ISimulation simulation)
    {
        return simulation.Name;
    }
}
