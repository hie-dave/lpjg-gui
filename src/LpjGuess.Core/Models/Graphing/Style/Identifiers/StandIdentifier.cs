using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identities;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies a series by its stand (only
/// applicable for a series containing stand-level data).
/// </summary>
public class StandIdentifier : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentityBase Identify(ISeriesData series)
    {
        if (series.Context.Stand is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by stand is only valid for stand-level model outputs");

        // TODO: should this take the gridcell into account?
        return new NumericIdentity(series.Context.Stand.Value);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByStand;
}
