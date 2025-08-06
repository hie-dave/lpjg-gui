using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identities;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies a series by its gridcell.
/// </summary>
public class GridcellIdentifier : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentityBase Identify(SeriesContext context)
    {
        return new GridcellIdentity(
            context.Gridcell.Latitude,
            context.Gridcell.Longitude,
            context.Gridcell.Name);
    }

    /// <summary>
    /// Identify a series by its gridcell.
    /// </summary>
    /// <param name="gridcell">The gridcell to identify.</param>
    /// <returns>The gridcell identity.</returns>
    public SeriesIdentityBase Identify(Gridcell gridcell)
    {
        return new GridcellIdentity(
            gridcell.Latitude,
            gridcell.Longitude,
            gridcell.Name);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByGridcell;
}
