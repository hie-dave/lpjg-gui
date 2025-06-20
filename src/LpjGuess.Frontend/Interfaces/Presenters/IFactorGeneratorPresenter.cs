using LpjGuess.Core.Interfaces.Factorial;

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
}
