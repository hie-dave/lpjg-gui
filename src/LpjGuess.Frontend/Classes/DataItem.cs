namespace LpjGuess.Frontend.Classes;

/// <summary>
/// Represents a data item in a grouped dropdown.
/// </summary>
public class DataItem<T> : IDropDownGroupItem
{
    /// <summary>
    /// A function which renders the value to a string.
    /// </summary>
    private readonly Func<T, string> renderer;

    /// <summary>
    /// Gets the underlying data value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Gets whether this item is selectable. Data items are selectable.
    /// </summary>
    public bool IsSelectable => true;

    /// <summary>
    /// Gets whether this item is in a group.
    /// </summary>
    public bool IsGrouped { get; private init; }

    /// <summary>
    /// Creates a new data item.
    /// </summary>
    /// <param name="value">The underlying data value.</param>
    /// <param name="renderer">Function to render the value as a string.</param>
    /// <param name="isGrouped">Whether this item is in a group.</param>
    public DataItem(T value, Func<T, string> renderer, bool isGrouped)
    {
        Value = value;
        this.renderer = renderer;
        IsGrouped = isGrouped;
    }

    /// <summary>
    /// Gets the display text for this data item.
    /// </summary>
    /// <returns>The rendered string representation of the value.</returns>
    public string GetDisplayText() => renderer(Value);
}
