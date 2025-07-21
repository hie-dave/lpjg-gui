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
    /// <typeparam name="TPresenter">The type of presenter to create.</typeparam>
    /// <typeparam name="TModel">The type of model to create.</typeparam>
    /// <param name="model">The model to pass to the presenter.</param>
    /// <returns>The created presenter.</returns>
    public TPresenter CreatePresenter<TPresenter, TModel>(TModel model)
        where TPresenter : IPresenter<TModel>
        where TModel : notnull;

    /// <summary>
    /// Create a series presenter for the given series.
    /// </summary>
    /// <typeparam name="TSeries">The type of the series.</typeparam>
    /// <param name="series">The series to present.</param>
    /// <returns>The series presenter.</returns>
    public ISeriesPresenter CreateSeriesPresenter<TSeries>(TSeries series)
        where TSeries : ISeries;

    /// <summary>
    /// Creates a presenter for the given model by automatically determining the appropriate presenter type.
    /// </summary>
    /// <typeparam name="TModel">The type of model to create a presenter for.</typeparam>
    /// <param name="model">The model to create a presenter for.</param>
    /// <returns>A presenter that can handle the given model.</returns>
    IPresenter<TModel> CreatePresenter<TModel>(TModel model) where TModel : notnull;

    /// <summary>
    /// Creates a presenter for the given model.
    /// </summary>
    /// <typeparam name="TPresenter">The type of presenter to create.</typeparam>
    /// <param name="model">The model to create a presenter for.</param>
    /// <returns>A presenter that can handle the given model.</returns>
    TPresenter CreatePresenter<TPresenter>(object model) where TPresenter : IPresenter;
}
