using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// A view which allows the user to edit a simple factor generator.
/// </summary>
public interface ISimpleFactorGeneratorView : IView
{
    /// <summary>
    /// Event which is raised when the simple factor generator changes.
    /// </summary>
    Event<IModelChange<SimpleFactorGenerator>> OnChanged { get; }

    /// <summary>
    /// Event which is raised when the user wants to add a new factor level.
    /// </summary>
    Event OnAddLevel { get; }

    /// <summary>
    /// Event which is raised when the user wants to remove a factor level. The
    /// event parameter is the factor view corresponding to the factor to be
    /// removed.
    /// </summary>
    Event<IView> OnRemoveLevel { get; }

    /// <summary>
    /// Populate the view with the given simple factor generator.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="factorLevelViews">The views to be used to display the factor levels.</param>
    void Populate(string name, IEnumerable<INamedView> factorLevelViews);

    /// <summary>
    /// Rename the specified factor view.
    /// </summary>
    /// <param name="view">The view to rename.</param>
    /// <param name="newName">The new name for the view.</param>
    void Rename(IView view, string newName);
}
