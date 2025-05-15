using OxyPlot;

namespace LpjGuess.Frontend.Data;

/// <summary>
/// Data for a single series.
/// </summary>
public class SeriesData
{
    /// <summary>
    /// The name of the series.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// The data points for the series.
    /// </summary>
    public IEnumerable<DataPoint> Data { get; private init; }

    /// <summary>
    /// Create a new <see cref="SeriesData"/> instance.
    /// </summary>
    /// <param name="name">The name of the series.</param>
    /// <param name="data">The data points for the series.</param>
    public SeriesData(string name, IEnumerable<DataPoint> data)
    {
        Name = name;
        Data = data;
    }
}
