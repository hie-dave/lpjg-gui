using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Data.Providers;

namespace LpjGuess.Frontend.Data;

/// <summary>
/// Factory for creating data providers.
/// </summary>
public static class DataProviderFactory
{
    /// <summary>
    /// Create a data provider for the specified data source.
    /// </summary>
    /// <typeparam name="T">The data source type.</typeparam>
    /// <param name="source">The data source instance.</param>
    /// <returns>A data provider for the specified data source.</returns>
    /// <exception cref="NotSupportedException">Thrown if the data source type is not supported.</exception>
    public static IDataProvider<T> Create<T>(T source) where T : class, IDataSource
    {
        if (source is ModelOutput)
        {
            return (IDataProvider<T>)new ModelOutputReader();
        }

        throw new NotSupportedException($"Data provider not supported for {typeof(T).Name}");
    }
}
