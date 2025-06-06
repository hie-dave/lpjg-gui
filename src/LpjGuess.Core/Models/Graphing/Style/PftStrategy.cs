using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A strategy for identifying PFTs in a deterministic way.
/// </summary>
public class PftStrategy : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentifierBase GetIdentifier(ISeriesData series)
    {
        if (series.Context.Pft is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by PFT is only valid for cohort-level model outputs");

        // TODO: should this take the gridcell/stand/patch into account?
        return new StringIdentifier(series.Context.Pft);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByPft;
}
