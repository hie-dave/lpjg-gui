using LpjGuess.Frontend.Interfaces.Presenters;
using Microsoft.Extensions.DependencyInjection;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// An interface for a factory that creates presenters for models.
/// </summary>
public interface IModelPresenterFactory<TInterface, TModel>
    where TInterface : IPresenter
    where TModel : notnull
{
    /// <summary>
    /// Creates a new presenter for the given model.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <returns>The created presenter.</returns>
    TInterface CreatePresenter(TModel model);
}

/// <summary>
/// A factory for creating presenters for models.
/// </summary>
public class ModelPresenterFactory<TInterface, TPresenter, TModel> : IModelPresenterFactory<TInterface, TModel>
    where TInterface : IPresenter
    where TPresenter : TInterface
    where TModel : notnull
{
    private readonly IServiceProvider provider;

    /// <summary>
    /// Creates a new instance of ModelPresenterFactory.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    public ModelPresenterFactory(IServiceProvider provider)
    {
        this.provider = provider;
    }

    /// <summary>
    /// Creates a new presenter for the given model.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <returns>The created presenter.</returns>
    public TInterface CreatePresenter(TModel model)
    {
        return ActivatorUtilities.CreateInstance<TPresenter>(provider, model);
    }
}
