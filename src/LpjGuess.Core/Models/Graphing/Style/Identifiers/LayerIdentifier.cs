using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies every series uniquely.
/// </summary>
public class LayerIdentifier : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentityBase Identify(SeriesContext context)
    {
        // TODO: should this take the gridcell/stand/patch into account?
        lock (this)
            return Identify(context.Layer);
    }

    /// <summary>
    /// Create a series identity for a given layer.
    /// </summary>
    /// <param name="layerName">The name of the layer.</param>
    /// <returns>The series identity.</returns>
    public SeriesIdentityBase Identify(string layerName)
    {
        return new StringIdentity(layerName);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByLayer;
}
