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
    /// Read data from the specified data source.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <returns>The data read from the data source.</returns>
    /// <exception cref="NotSupportedException">Thrown if the data source type is not supported.</exception>
    public static IEnumerable<SeriesData> Read(IDataSource source)
    {
        if (source is ModelOutput modelOutput)
        {
            return new ModelOutputReader().Read(modelOutput);
        }

        throw new NotSupportedException($"Data provider not supported for {typeof(IDataSource).Name}");
    }

    /// <summary>
    /// Get the name of the data source.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <returns>The name of the data source.</returns>
    public static string GetName(IDataSource source)
    {
        // fixme!!
        if (source is ModelOutput modelOutput)
            return new ModelOutputReader().GetName(modelOutput);

        throw new NotSupportedException($"Data provider not supported for {typeof(IDataSource).Name}");
    }
}
