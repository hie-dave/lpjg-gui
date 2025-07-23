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
        IDataSourcePresenter presenter = (IDataSourcePresenter)CreatePresenterDynamic(series.DataSource);
        Type presenterType = typeof(SeriesPresenter<>).MakeGenericType(series.GetType());
        return (ISeriesPresenter)ActivatorUtilities.CreateInstance(serviceProvider, presenterType, view, series, presenter);
    }

    /// <inheritdoc />
    public IPresenter CreatePresenter<TModel>(TModel model)
        where TModel : notnull
    {
        return CreatePresenterDynamic(model);
    }

    /// <inheritdoc />
    public TPresenter CreatePresenter<TPresenter>(object model) where TPresenter : IPresenter
    {
        return (TPresenter)CreatePresenterDynamic(model, typeof(TPresenter));
    }

    /// <summary>
    /// Create a presenter for the given model, whose type is not known at
    /// compile time.
    /// </summary>
    /// <param name="model">The model to create a presenter for.</param>
    /// <returns>The presenter.</returns>
    private IPresenter CreatePresenterDynamic<TModel>(TModel model)
        where TModel : notnull
    {
        Type modelType = model.GetType();
        Type presenterType = typeof(IPresenter<>).MakeGenericType(modelType);
        return (IPresenter)CreatePresenterDynamic(model, presenterType);
    }

    private object CreatePresenterDynamic<TModel>(TModel model, Type presenterType)
        where TModel : notnull
    {
        MethodInfo method = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == nameof(CreatePresenter) &&
            m.IsGenericMethod &&
            m.GetGenericArguments().Length == 2 &&
            m.GetParameters().Length == 1)
            .Single();
        method = method.MakeGenericMethod(presenterType, model.GetType());
        return method.Invoke(this, [model])!;
    }
}
