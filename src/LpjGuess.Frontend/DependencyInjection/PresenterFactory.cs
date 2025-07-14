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
        Console.WriteLine($"Creating presenter {typeof(TPresenter).ToFriendlyName()} for model of type {model.GetType().ToFriendlyName()}");
        return ActivatorUtilities.CreateInstance<TPresenter>(serviceProvider, model);
    }

    /// <inheritdoc />
    public ISeriesPresenter CreateSeriesPresenter<TSeries>(TSeries series, IEnumerable<string> instructionFiles)
        where TSeries : ISeries
    {
        IDataSourcePresenter presenter = CreateDataSourcePresenter(series.DataSource, instructionFiles);
        return ActivatorUtilities.CreateInstance<ISeriesPresenter<TSeries>>(serviceProvider, presenter);
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
        Console.WriteLine($"Creating presenter {typeof(TPresenter).ToFriendlyName()} for model of type {model.GetType().ToFriendlyName()}");
        return serviceProvider.GetRequiredService<TPresenter>();
    }

    /// <summary>
    /// Create a data source presenter for the given data source.
    /// </summary>
    /// <typeparam name="TDataSource">The type of the data source.</typeparam>
    /// <param name="dataSource">The data source to present.</param>
    /// <param name="instructionFiles">The instruction files in the workspace.</param>
    /// <returns>The data source presenter.</returns>
    private IDataSourcePresenter CreateDataSourcePresenter<TDataSource>(TDataSource dataSource, IEnumerable<string> instructionFiles)
        where TDataSource : IDataSource
    {
        IDataSourcePresenter presenter = ActivatorUtilities.CreateInstance<IDataSourcePresenter<TDataSource>>(serviceProvider, dataSource, instructionFiles);
        return presenter;
    }
}
