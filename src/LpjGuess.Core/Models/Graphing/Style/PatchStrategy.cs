using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A strategy for identifying patches in a deterministic way.
/// </summary>
public class PatchStrategy : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentifierBase GetIdentifier(ISeriesData series)
    {
        if (series.Context.Patch is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by patch is only valid for patch-level model outputs");

        // TODO: should this take the gridcell/stand into account?
        return new NumericIdentifier(series.Context.Patch.Value);
    }
}
