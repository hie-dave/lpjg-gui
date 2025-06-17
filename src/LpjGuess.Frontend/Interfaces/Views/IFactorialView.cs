using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a factorial view.
/// </summary>
public interface IFactorialView : IView
{
    /// <summary>
    /// Event which is raised when the factorial generator changes.
    /// </summary>
    Event<IModelChange<FactorialGenerator>> OnChanged { get; }

    /// <summary>
    /// Populate the view with the given factorial generator.
    /// </summary>
    /// <param name="fullFactorial">Whether to generate a full factorial.</param>
    void Populate(bool fullFactorial);
}
