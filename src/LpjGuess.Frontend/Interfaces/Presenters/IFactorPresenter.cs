using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface for a presenter which manages a factor.
/// </summary>
public interface IFactorPresenter : IPresenter
{
    /// <summary>
    /// The factor being presented.
    /// </summary>
    IFactor Model { get; }

    /// <summary>
    /// The view being presented.
    /// </summary>
    IView View { get; }

    /// <summary>
    /// Event which is raised when the name of the factor has been changed by the user.
    /// </summary>
    Event<string> OnRenamed { get; }
}
