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
}
