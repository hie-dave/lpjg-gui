using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identities;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies a series by its patch (only
/// applicable for a series containing patch-level data).
/// </summary>
public class PatchIdentifier : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentityBase Identify(ISeriesData series)
    {
        if (series.Context.Patch is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by patch is only valid for patch-level model outputs");

        // TODO: should this take the gridcell/stand into account?
        return new NumericIdentity(series.Context.Patch.Value);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByPatch;
}
