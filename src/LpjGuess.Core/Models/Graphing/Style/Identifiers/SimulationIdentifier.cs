using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies a series by its simulation (only
/// applicable for series generated from model outputs).
/// </summary>
public class SimulationIdentifier : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentityBase Identify(SeriesContext context)
    {
        if (context.SimulationName is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by simulation is only valid for model output series");

        return Identify(context.SimulationName);
    }

    /// <summary>
    /// Create a series identity for a given simulation.
    /// </summary>
    /// <param name="simulationName">The name of the simulation.</param>
    /// <returns>The series identity.</returns>
    public SeriesIdentityBase Identify(string simulationName)
    {
        return new StringIdentity(simulationName);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.BySimulation;
}
