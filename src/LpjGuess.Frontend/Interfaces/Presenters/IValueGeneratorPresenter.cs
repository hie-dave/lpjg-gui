using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface for a presenter which manages a value generator.
/// </summary>
public interface IValueGeneratorPresenter : IPresenter
{
    /// <summary>
    /// The value generator being presented.
    /// </summary>
    IValueGenerator Model { get; }

    /// <summary>
    /// The view being presented.
    /// </summary>
    IView View { get; }

    /// <summary>
    /// Called when the data type has been changed by the user. The event
    /// parameter is the new value generator instance.
    /// </summary>
    Event<IValueGenerator> OnTypeChanged { get; }
}
