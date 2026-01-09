namespace LpjGuess.Core.Models;

/// <summary>
/// The top-level index for simulations under an output directory.
/// </summary>
public class SimulationIndex
{
    /// <summary>
    /// Paths to encapsulated simulations, relative to the index.
    /// </summary>
    public IReadOnlyList<string> Simulations { get; private init; }

    /// <summary>
    /// Creates a default <see cref="SimulationIndex"/>.
    /// </summary>
    /// <param name="simulations">Paths to encapsulated simulations.</param>
    public SimulationIndex(IEnumerable<string> simulations)
    {
        Simulations = simulations.ToList();
    }
}
