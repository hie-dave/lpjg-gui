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
        return CreatePresenterInternal<TPresenter, TModel>(model);
    }

    /// <inheritdoc />
    public ISeriesPresenter CreateSeriesPresenter<TSeries>(TSeries series)
        where TSeries : ISeries
    {
        Type viewType = typeof(ISeriesView<>).MakeGenericType(series.GetType());
        object view = serviceProvider.GetRequiredService(viewType);
        IDataSourcePresenter presenter = CreatePresenter<IDataSourcePresenter>(series.DataSource);
        Type presenterType = typeof(SeriesPresenter<>).MakeGenericType(series.GetType());
        return (ISeriesPresenter)ActivatorUtilities.CreateInstance(serviceProvider, presenterType, view, series, presenter);
    }

    /// <inheritdoc />
    public TPresenter CreatePresenter<TPresenter>(object model) where TPresenter : IPresenter
    {
        return (TPresenter)CreatePresenterDynamic(model, typeof(TPresenter));
    }

    private object CreatePresenterDynamic<TModel>(TModel model, Type presenterType)
        where TModel : notnull
    {
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        MethodInfo method = GetType().GetMethod(nameof(CreatePresenterInternal), flags)!;
        method = method.MakeGenericMethod(presenterType, model.GetType());
        return method.Invoke(this, [model])!;
    }

    /// <summary>
    /// Create a presenter for the given model.
    /// </summary>
    /// <typeparam name="TPresenter">The type of the presenter.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="model">The model to present.</param>
    /// <returns>The created presenter.</returns>
    private TPresenter CreatePresenterInternal<TPresenter, TModel>(TModel model)
        where TPresenter : IPresenter
        where TModel : notnull
    {
        IModelPresenterFactory<TPresenter, TModel> factory = serviceProvider.GetRequiredService<IModelPresenterFactory<TPresenter, TModel>>();
        return factory.CreatePresenter(model);
    }
}
