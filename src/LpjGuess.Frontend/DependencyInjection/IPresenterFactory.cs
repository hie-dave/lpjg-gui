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
    /// <typeparam name="TView">The type of view to create.</typeparam>
    /// <typeparam name="TModel">The type of model to create.</typeparam>
    /// <param name="model">The model to pass to the presenter.</param>
    /// <returns>The created presenter.</returns>
    public TPresenter CreatePresenter<TPresenter, TView, TModel>(TModel model)
        where TPresenter : IPresenter<TView, TModel>
        where TView : IView
        where TModel : notnull;
}
