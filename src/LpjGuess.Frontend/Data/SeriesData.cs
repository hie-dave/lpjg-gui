using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models;
using OxyPlot;

namespace LpjGuess.Frontend.Data;

/// <summary>
/// Data for a single series.
/// </summary>
public class SeriesData : ISeriesData
{
    private readonly IReadOnlyList<DataPoint> data;

    /// <summary>
    /// The name of the series.
    /// </summary>
    public string Name { get; private init; }

    /// <inheritdoc />
    public SeriesContext Context { get; private init; }

    /// <summary>
    /// The data points for the series.
    /// </summary>
    public IEnumerable<DataPoint> Data => data;

    /// <summary>
    /// Values used to align records with another data source.
    /// </summary>
    public IReadOnlyList<double> MatchValues { get; }

    /// <summary>
    /// Create a new <see cref="SeriesData"/> instance.
    /// </summary>
    /// <param name="name">The name of the series.</param>
    /// <param name="context">The context of the series.</param>
    /// <param name="data">The data points for the series.</param>
    /// <param name="matchValues">Values used to align these points with another data source.</param>
    public SeriesData(
        string name,
        SeriesContext context,
        IEnumerable<DataPoint> data,
        IEnumerable<double>? matchValues = null)
    {
        Name = name;
        Context = context;
        this.data = data.ToList();
        MatchValues = matchValues?.ToList() ?? this.data.Select(point => point.X).ToList();
        if (MatchValues.Count != this.data.Count)
            throw new ArgumentException("Match values must correspond one-to-one with data points.", nameof(matchValues));
    }
}
