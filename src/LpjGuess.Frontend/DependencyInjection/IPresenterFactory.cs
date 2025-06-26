using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// Interface to a class which can create presenters.
/// </summary>
public interface IPresenterFactory
{
    /// <summary>
    /// Create a presenter of the specified type.
    /// </summary>
    /// <typeparam name="TPresenter">The type of presenter to create.</typeparam>
    /// <returns>The created presenter.</returns>
    TPresenter CreatePresenter<TPresenter>() where TPresenter : IPresenter;
}
