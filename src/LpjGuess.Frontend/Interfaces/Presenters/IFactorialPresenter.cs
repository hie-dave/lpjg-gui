using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which controls a factorial view to display the
/// contents of a factorial generator.
/// </summary>
public interface IFactorialPresenter : IPresenter<IFactorialView, FactorialGenerator>
{
    /// <summary>
    /// Event which is raised when the factorial generator changes.
    /// </summary>
    Event OnChanged { get; }

    /// <summary>
    /// Populate the presenter with the given factorial generator.
    /// </summary>
    void Refresh();
}
