using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Graphing.Style;
using Newtonsoft.Json;

namespace LpjGuess.Core.Models.Graphing.Series;

/// <summary>
/// A series which renders each data value as an unconnected point.
/// </summary>
[Serializable]
public class ScatterSeries : SeriesBase
{
    /// <summary>
    /// Create a new <see cref="ScatterSeries"/> instance.
    /// </summary>
    /// <param name="title">The title of the series.</param>
    /// <param name="colourProvider">The colour provider.</param>
    /// <param name="dataSource">The data source for the series.</param>
    /// <param name="xAxisPosition">The position of the X axis for the series.</param>
    /// <param name="yAxisPosition">The position of the Y axis for the series.</param>
    [JsonConstructor]
    public ScatterSeries(
        string title,
        IStyleProvider<Colour> colourProvider,
        IDataSource dataSource,
        AxisPosition xAxisPosition,
        AxisPosition yAxisPosition)
        : base(title, colourProvider, dataSource, xAxisPosition, yAxisPosition)
    {
    }

    /// <summary>
    /// Create a scatter series with independent x- and y-axis data sources.
    /// </summary>
    public ScatterSeries(
        string title,
        IStyleProvider<Colour> colourProvider,
        IDataSource xDataSource,
        IDataSource yDataSource,
        AxisPosition xAxisPosition,
        AxisPosition yAxisPosition)
        : base(title, colourProvider, xDataSource, yDataSource, xAxisPosition, yAxisPosition)
    {
    }
}
