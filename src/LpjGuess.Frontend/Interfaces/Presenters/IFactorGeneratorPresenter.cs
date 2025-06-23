using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface for a presenter which manages a factor generator.
/// </summary>
public interface IFactorGeneratorPresenter : IPresenter
{
    /// <summary>
    /// The factor generator being presented.
    /// </summary>
    IFactorGenerator Model { get; }

    /// <summary>
    /// The view being presented.
    /// </summary>
    IView View { get; }

    /// <summary>
    /// Name of the factor.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Called when the name of the factor has been changed by the user.
    /// </summary>
    Event<string> OnRenamed { get; }
}
