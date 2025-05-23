namespace LpjGuess.Frontend.Classes;

/// <summary>
/// Represents a group header in a grouped dropdown.
/// </summary>
public class GroupHeaderItem : IDropDownGroupItem
{
    /// <summary>
    /// Gets the name of the group.
    /// </summary>
    public string GroupName { get; }

    /// <summary>
    /// Gets whether this item is selectable. Headers are not selectable.
    /// </summary>
    public bool IsSelectable => false;

    /// <summary>
    /// Creates a new group header item.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    public GroupHeaderItem(string groupName) => GroupName = groupName;

    /// <summary>
    /// Gets the display text for this header item.
    /// </summary>
    /// <returns>The group name.</returns>
    public string GetDisplayText() => GroupName;
}
