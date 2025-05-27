using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A strategy for identifying simulations in a deterministic way.
/// </summary>
public class SimulationStrategy : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentifierBase GetIdentifier(ISeriesData series)
    {
        if (series.Context.SimulationName is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by simulation is only valid for model output series");

        return new SimulationIdentifier(series.Context.SimulationName);
    }
}
