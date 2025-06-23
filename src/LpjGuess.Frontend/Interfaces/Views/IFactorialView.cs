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
    /// Event which is raised when the user wants to add a factor.
    /// </summary>
    Event OnAddFactor { get; }

    /// <summary>
    /// Event which is raised when the user wants to remove a factor. The event
    /// parameter is the name of the factor to be removed.
    /// </summary>
    Event<string> OnRemoveFactor { get; }

    /// <summary>
    /// Populate the view with the given factorial generator.
    /// </summary>
    /// <param name="fullFactorial">Whether to generate a full factorial.</param>
    /// <param name="factorViews">The views for the factors of this factorial.</param>
    void Populate(bool fullFactorial, IEnumerable<INamedView> factorViews);

    /// <summary>
    /// Rename a factor in the view.
    /// </summary>
    /// <param name="view">The view of the factor to rename.</param>
    /// <param name="name">The new name of the factor.</param>
    void RenameFactor(IView view, string name);
}
