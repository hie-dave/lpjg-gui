using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// Interface to a class which can create presenters.
/// </summary>
public interface IPresenterFactory
{
    /// <summary>
    /// Create a presenter of the specified type which doesn't manage a model.
    /// </summary>
    /// <typeparam name="TPresenter">The type of presenter to create.</typeparam>
    /// <returns>The created presenter.</returns>
    TPresenter CreatePresenter<TPresenter>() where TPresenter : IPresenter;

    /// <summary>
    /// Create a presenter of the specified type.
    /// </summary>
    /// <remarks>
    /// For this to work, the model type must match exactly what is registered
    /// in the DI container. E.g. it should be ModelOutput, not IDataSource.
    /// </remarks>
    /// <typeparam name="TPresenter">The type of presenter to create.</typeparam>
    /// <typeparam name="TModel">The type of model to create.</typeparam>
    /// <param name="model">The model to pass to the presenter.</param>
    /// <returns>The created presenter.</returns>
    public TPresenter CreatePresenter<TPresenter, TModel>(TModel model)
        where TPresenter : IPresenter<TModel>
        where TModel : notnull;

    /// <summary>
    /// Creates a presenter for the given model, whose type is not known at
    /// compile time.
    /// </summary>
    /// <typeparam name="TPresenter">The type of presenter to create.</typeparam>
    /// <param name="model">The model to create a presenter for.</param>
    /// <returns>The created presenter.</returns>
    TPresenter CreatePresenter<TPresenter>(object model) where TPresenter : IPresenter;

    /// <summary>
    /// Create a series presenter for the given series.
    /// </summary>
    /// <typeparam name="TSeries">The type of the series.</typeparam>
    /// <param name="series">The series to present.</param>
    /// <returns>The series presenter.</returns>
    public ISeriesPresenter CreateSeriesPresenter<TSeries>(TSeries series)
        where TSeries : ISeries;
}
