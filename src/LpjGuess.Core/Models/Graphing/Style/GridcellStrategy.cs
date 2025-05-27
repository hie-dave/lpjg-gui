using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A strategy for identifying gridcells in a deterministic way.
/// </summary>
public class GridcellStrategy : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentifierBase GetIdentifier(ISeriesData series)
    {
        return new GridcellIdentifier(
            series.Context.Gridcell.Latitude,
            series.Context.Gridcell.Longitude,
            series.Context.Gridcell.Name);
    }
}
