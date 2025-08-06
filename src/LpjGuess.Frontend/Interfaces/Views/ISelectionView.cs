using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface for views that allow the user to select one or more items.
/// </summary>
public interface ISelectionView : IView
{
    /// <summary>
    /// The event that is raised when the selection is changed by the user.
    /// </summary>
    Event<IEnumerable<string>> OnSelectionChanged { get; }
    
    /// <summary>
    /// Populate the view with the given items.
    /// </summary>
    /// <param name="items">The complete set of items to populate the view with.</param>
    void Populate(IEnumerable<string> items);

    /// <summary>
    /// Select the given items.
    /// </summary>
    /// <param name="items">The items which should be displayed as "selected".</param>
    void Select(IEnumerable<string> items);

    /// <summary>
    /// Gets the currently selected items.
    /// </summary>
    /// <returns>The currently selected items.</returns>
    IEnumerable<string> GetSelectedItems();
}
