namespace LpjGuess.Core.Interfaces.Factorial;

/// <summary>
/// Interface to a class which generates simulations represented by changes to
/// an arbitrary base simulation.
/// </summary>
public interface ISimulationGenerator
{
    /// <summary>
    /// Generate the simulations encapsulated by this generator.
    /// </summary>
    /// <returns>The simulations.</returns>
    IEnumerable<ISimulation> Generate();
}
