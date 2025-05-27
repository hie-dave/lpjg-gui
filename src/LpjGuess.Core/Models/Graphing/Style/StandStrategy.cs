using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A strategy for identifying stands in a deterministic way.
/// </summary>
public class StandStrategy : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentifierBase GetIdentifier(ISeriesData series)
    {
        if (series.Context.Stand is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by stand is only valid for stand-level model outputs");

        // TODO: should this take the gridcell into account?
        return new NumericIdentifier(series.Context.Stand.Value);
    }
}
