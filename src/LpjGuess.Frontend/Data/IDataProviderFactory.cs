using LpjGuess.Core.Interfaces;

namespace LpjGuess.Frontend.Data;

/// <summary>
/// Interface to a factory which generates data providers.
/// </summary>
public interface IDataProviderFactory
{
    /// <summary>
    /// Read data from the specified data source.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The data read from the data source.</returns>
    public Task<IEnumerable<SeriesData>> ReadAsync(IDataSource source, CancellationToken ct);

    /// <summary>
    /// Get the name of the data source.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <returns>The name of the data source.</returns>
    public string GetName(IDataSource source);
}
