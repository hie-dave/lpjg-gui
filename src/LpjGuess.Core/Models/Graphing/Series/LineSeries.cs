using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Graphing.Style;
using Newtonsoft.Json;

namespace LpjGuess.Core.Models.Graphing.Series;

/// <summary>
/// A base series class for all graph series types.
/// </summary>
[Serializable]
public class LineSeries : SeriesBase
{
    /// <summary>
    /// The line type provider.
    /// </summary>
    public IStyleProvider<LineType> Type { get; set; }

    /// <summary>
    /// The line thickness provider.
    /// </summary>
    public IStyleProvider<LineThickness> Thickness { get; set; }

    /// <summary>
    /// Create a new <see cref="LineSeries"/> instance.
    /// </summary>
    /// <param name="title">The title of the series.</param>
    /// <param name="colourProvider">The colour provider.</param>
    /// <param name="dataSource">The data source for the series.</param>
    /// <param name="xAxisPosition">The position of the X axis for the series.</param>
    /// <param name="yAxisPosition">The position of the Y axis for the series.</param>
    /// <param name="typeProvider">The line type provider.</param>
    /// <param name="thicknessProvider">The line thickness provider.</param>
    [JsonConstructor]
    public LineSeries(
        string title,
        IStyleProvider<Colour> colourProvider,
        IDataSource dataSource,
        AxisPosition xAxisPosition,
        AxisPosition yAxisPosition,
        IStyleProvider<LineType> typeProvider,
        IStyleProvider<LineThickness> thicknessProvider)
        : base(title, colourProvider, dataSource, xAxisPosition, yAxisPosition)
    {
        Type = typeProvider;
        Thickness = thicknessProvider;
    }
}
