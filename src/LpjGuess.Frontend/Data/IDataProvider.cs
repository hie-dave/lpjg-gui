using LpjGuess.Core.Interfaces;

namespace LpjGuess.Frontend.Data;

/// <summary>
/// An interface to a data provider. The type parameter is the type of data
/// source supported by this data provider.
/// </summary>
public interface IDataProvider<T> where T : IDataSource
{
    /// <summary>
    /// Read data from the data source.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <returns>The data read from the data source.</returns>
    Task<IEnumerable<SeriesData>> ReadAsync(T source);

    /// <summary>
    /// Generate a name for the data source which would be suitable for display
    /// on a graph.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <returns>A name which describes the data returned form this data source.</returns>
    string GetName(T source);
}
