using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A class which uniquely identifies a simulation based on its name.
/// </summary>
public class SimulationIdentifier : SeriesIdentifierBase
{
    /// <summary>
    /// The name of the simulation.
    /// </summary>
    public string SimulationName { get; private init; }

    /// <summary>
    /// Create a new <see cref="SimulationIdentifier"/> instance.
    /// </summary>
    /// <param name="simulationName">The name of the simulation.</param>
    public SimulationIdentifier(string simulationName)
    {
        SimulationName = simulationName;
    }

    /// <inheritdoc />
    public override bool Equals(SeriesIdentifierBase? other)
    {
        if (other is not SimulationIdentifier otherIdentifier)
            return false;

        return SimulationName == otherIdentifier.SimulationName;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return SimulationName.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return SimulationName;
    }
}
