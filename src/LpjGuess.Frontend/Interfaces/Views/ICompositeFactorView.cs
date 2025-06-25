using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view which allows the user to edit a composite factor.
/// </summary>
public interface ICompositeFactorView : IView
{
    /// <summary>
    /// Event which is raised when the composite factor changes.
    /// </summary>
    Event<IModelChange<CompositeFactor>> OnChanged { get; }

    /// <summary>
    /// Event which is raised when the user wants to add a new factor.
    /// </summary>
    Event OnAddFactor { get; }

    /// <summary>
    /// Event which is raised when the user wants to remove a factor. The
    /// event parameter is the name of the factor view corresponding to the
    /// factor to be removed.
    /// </summary>
    Event<string> OnRemoveFactor { get; }

    /// <summary>
    /// Populate the view.
    /// </summary>
    /// <param name="name">The name of the generated factor.</param>
    /// <param name="factorViews">The views to be used to display the factors.</param>
    void Populate(string name, IEnumerable<INamedView> factorViews);

    /// <summary>
    /// Change the factor name displayed for this composite factor.
    /// </summary>
    /// <param name="name">The new name.</param>
    void Rename(string name);
}
