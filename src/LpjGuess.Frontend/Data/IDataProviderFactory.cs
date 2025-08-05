using LpjGuess.Core.Interfaces;

namespace LpjGuess.Frontend.Data;

/// <summary>
/// Interface to a factory which generates data providers.
/// </summary>
public interface IDataProviderFactory
{
    /// <summary>
    /// Read data from the data source.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The data read from the data source.</returns>
    Task<IEnumerable<SeriesData>> ReadAsync(IDataSource source, CancellationToken ct);

    /// <summary>
    /// Generate a name for the data source which would be suitable for display
    /// on a graph.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <returns>A name which describes the data returned form this data source.</returns>
    string GetName(IDataSource source);

    /// <summary>
    /// Get the number of series yielded by a data source.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <returns>The number of series in the data source.</returns>
    int GetNumSeries(IDataSource source);
}
