using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies every series uniquely.
/// </summary>
public class LayerIdentifier : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentityBase Identify(ISeriesData series)
    {
        // TODO: should this take the gridcell/stand/patch into account?
        lock (this)
            return new StringIdentity(series.Context.Layer);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByLayer;
}
