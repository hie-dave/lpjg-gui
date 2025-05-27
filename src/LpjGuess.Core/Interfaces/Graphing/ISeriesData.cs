using Dave.Benchmarks.Core.Models.Entities;
using LpjGuess.Core.Models;

namespace LpjGuess.Core.Interfaces.Graphing;

/// <summary>
/// Interface to the data behind a series on a graph.
/// </summary>
public interface ISeriesData
{
    /// <summary>
    /// The context of the series.
    /// </summary>
    SeriesContext Context { get; }
}
