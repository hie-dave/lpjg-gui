namespace LpjGuess.Frontend.Classes;

/// <summary>
/// Represents an item in a grouped dropdown.
/// </summary>
public interface IDropDownGroupItem
{
    /// <summary>
    /// Gets the display text for this item.
    /// </summary>
    string GetDisplayText();

    /// <summary>
    /// Whether this item is selectable.
    /// </summary>
    bool IsSelectable { get; }
}
