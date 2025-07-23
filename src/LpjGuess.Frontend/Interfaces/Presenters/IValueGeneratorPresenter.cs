using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface for a presenter which manages a value generator.
/// </summary>
public interface IValueGeneratorPresenter : IPresenter<IValueGenerator>
{
    /// <summary>
    /// Called when the data type has been changed by the user. The event
    /// parameter is the new value generator instance.
    /// </summary>
    Event<IValueGenerator> OnTypeChanged { get; }

    /// <summary>
    /// Called when the model has been changed by the user.
    /// </summary>
    Event OnChanged { get; }
}
