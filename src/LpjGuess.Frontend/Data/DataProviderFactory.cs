using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Runner.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LpjGuess.Frontend.Data;

/// <summary>
/// Factory for creating data providers.
/// </summary>
public class DataProviderFactory : IDataProviderFactory
{
    /// <summary>
    /// The service provider.
    /// </summary>
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Create a new <see cref="DataProviderFactory"/> instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public DataProviderFactory(
        IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SeriesData>> ReadAsync(IDataSource source, CancellationToken ct)
    {
        return await ReadAsyncGeneric((dynamic)source, ct);
    }

    /// <inheritdoc />
    public string GetName(IDataSource source)
    {
        return GetNameGeneric((dynamic)source);
    }

    /// <inheritdoc />
    public int GetNumSeries(IDataSource source)
    {
        return GetNumSeriesGeneric((dynamic)source);
    }

    private async Task<IEnumerable<SeriesData>> ReadAsyncGeneric<T>(T source, CancellationToken ct)
        where T : IDataSource
    {
        return await CreateProvider(source).ReadAsync(source, ct);
    }

    private string GetNameGeneric<T>(T source)
        where T : IDataSource
    {
        return CreateProvider(source).GetName(source);
    }

    private int GetNumSeriesGeneric<T>(T source)
        where T : IDataSource
    {
        return CreateProvider(source).GetNumSeries(source);
    }

    private IDataProvider<T> CreateProvider<T>(T dataSource) where T : IDataSource
    {
        Type interfaceType = typeof(IDataProvider<>).MakeGenericType(dataSource.GetType());
        return (IDataProvider<T>)serviceProvider.GetRequiredService(interfaceType);
    }
}
