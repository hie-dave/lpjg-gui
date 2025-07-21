using System.Reflection;
using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Presenters;
using Microsoft.Extensions.DependencyInjection;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// Factory for creating presenters via dependency injection.
/// </summary>
public class PresenterFactory : IPresenterFactory
{
    /// <summary>
    /// The service provider.
    /// </summary>
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Creates a new instance of PresenterFactory.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public PresenterFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public TPresenter CreatePresenter<TPresenter>() where TPresenter : IPresenter
    {
        return serviceProvider.GetRequiredService<TPresenter>();
    }

    /// <inheritdoc />
    public TPresenter CreatePresenter<TPresenter, TModel>(TModel model)
        where TPresenter : IPresenter<TModel>
        where TModel : notnull
    {
        IModelPresenterFactory<TPresenter, TModel> factory = serviceProvider.GetRequiredService<IModelPresenterFactory<TPresenter, TModel>>();
        return factory.CreatePresenter(model);
    }

    /// <inheritdoc />
    public ISeriesPresenter CreateSeriesPresenter<TSeries>(TSeries series)
        where TSeries : ISeries
    {
        Type viewType = typeof(ISeriesView<>).MakeGenericType(series.GetType());
        object view = serviceProvider.GetRequiredService(viewType);
        IDataSourcePresenter presenter = CreateDataSourcePresenter(series.DataSource);
        Type presenterType = typeof(SeriesPresenter<>).MakeGenericType(series.GetType());
        return (ISeriesPresenter)ActivatorUtilities.CreateInstance(serviceProvider, presenterType, view, series, presenter);
    }

    /// <inheritdoc />
    public IPresenter<TModel> CreatePresenter<TModel>(TModel model)
        where TModel : notnull
    {
        // Get the model type.
        return CreatePresenter<IPresenter<TModel>>(model);
    }

    /// <inheritdoc />
    public TPresenter CreatePresenter<TPresenter>(object model) where TPresenter : IPresenter
    {
        return serviceProvider.GetRequiredService<TPresenter>();
    }

    /// <summary>
    /// Create a data source presenter for the given data source.
    /// </summary>
    /// <typeparam name="TDataSource">The type of the data source.</typeparam>
    /// <param name="dataSource">The data source to present.</param>
    /// <returns>The data source presenter.</returns>
    private IDataSourcePresenter CreateDataSourcePresenter<TDataSource>(TDataSource dataSource)
        where TDataSource : IDataSource
    {
        Type modelType = dataSource.GetType();
        Type presenterType = typeof(IDataSourcePresenter<>).MakeGenericType(modelType);
        MethodInfo methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == nameof(CreatePresenter) &&
            m.IsGenericMethod &&
            m.GetGenericArguments().Length == 2 &&
            m.GetParameters().Length == 1)
            .Single();
        MethodInfo genericMethod = methods.MakeGenericMethod(presenterType, modelType);
        return (IDataSourcePresenter)genericMethod.Invoke(this, [dataSource])!;
    }
}
