using LpjGuess.Core.Interfaces;

namespace LpjGuess.Core.Models.Graphing.Series;

/// <summary>
/// A base series class for all graph series types.
/// </summary>
[Serializable]
public class LineSeries : SeriesBase
{
    /// <summary>
    /// The type of line to use for the series.
    /// </summary>
    public LineType Type { get; set; }

    /// <summary>
    /// The thickness of the line.
    /// </summary>
    public LineThickness Thickness { get; set; }

    /// <summary>
    /// Create a new <see cref="LineSeries"/> instance.
    /// </summary>
    public LineSeries()
    {
        Type = LineType.Solid;
        Thickness = LineThickness.Regular;
    }

    /// <summary>
    /// Create a new <see cref="LineSeries"/> instance.
    /// </summary>
    /// <param name="title">The title of the series.</param>
    /// <param name="colour">The colour of the series.</param>
    /// <param name="dataSource">The data source for the series.</param>
    /// <param name="xAxisPosition">The position of the X axis for the series.</param>
    /// <param name="yAxisPosition">The position of the Y axis for the series.</param>
    /// <param name="type">The type of line to use for the series.</param>
    /// <param name="thickness">The thickness of the line.</param>
    public LineSeries(
        string title,
        string colour,
        IDataSource dataSource,
        AxisPosition xAxisPosition,
        AxisPosition yAxisPosition,
        LineType type,
        LineThickness thickness)
        : base(title, colour, dataSource, xAxisPosition, yAxisPosition)
    {
        Type = type;
        Thickness = thickness;
    }
}
