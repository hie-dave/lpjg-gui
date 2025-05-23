using Gtk;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Views.Helpers;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A dropdown view that displays items grouped by a specified key selector.
/// </summary>
/// <remarks>
/// This dropdown implementation is fundamentally tied to rendering entities as
/// strings via labels. This could be made more abstract in the future by making
/// the class generic on the widget type, similar to the base class.
/// </remarks>
/// <typeparam name="T">The type of data items in the dropdown.</typeparam>
public class GroupedDropDownView<T> : DropDownView<IDropDownGroupItem, Label> where T : class
{
    /// <summary>
    /// A function which extracts the group key from an item.
    /// </summary>
    private readonly Func<T, string> groupKeySelector;

    /// <summary>
    /// A function which renders an item as a string.
    /// </summary>
    private readonly Func<T, string> itemRenderer;

    /// <summary>
    /// If true, group headers will always be shown, even if there is only one
    /// item in the group. If false, group headers will be shown only for groups
    /// with multiple items.
    /// </summary>
    private readonly bool alwaysShowHeaders;

    /// <summary>
    /// Called when a data item is selected.
    /// </summary>
    public Event<T> OnDataItemSelected { get; private init; }

    /// <summary>
    /// Creates a new <see cref="GroupedDropDownView{T}"/> instance.
    /// </summary>
    /// <param name="groupKeySelector">Function to extract the group key from an item.</param>
    /// <param name="itemRenderer">Function to render an item as a string.</param>
    /// <param name="alwaysShowHeaders">If true, group headers will always be shown, even if there is only one item in the group. If false, group headers will be shown only for groups with multiple items.</param>
    public GroupedDropDownView(
        Func<T, string> groupKeySelector,
        Func<T, string> itemRenderer,
        bool alwaysShowHeaders) : base()
    {
        this.groupKeySelector = groupKeySelector;
        this.itemRenderer = itemRenderer;
        this.alwaysShowHeaders = alwaysShowHeaders;
        OnDataItemSelected = new Event<T>();
        OnSelectionChanged.ConnectTo(HandleSelectionChanged);
    }

    /// <summary>
    /// Handles selection changes in the dropdown.
    /// </summary>
    /// <param name="item">The selected item.</param>
    private void HandleSelectionChanged(IDropDownGroupItem item)
    {
        if (item is DataItem<T> dataItem)
            OnDataItemSelected.Invoke(dataItem.Value);
    }

    /// <summary>
    /// Selects a data item in the dropdown.
    /// </summary>
    /// <param name="item">The data item to select.</param>
    public void Select(T item)
    {
        // Find the corresponding DropdownGroupItem
        for (uint i = 0; i < widget.Model.GetNItems(); i++)
        {
            var obj = widget.Model.GetObject(i);
            if (obj is GenericGObject<IDropDownGroupItem> wrapper &&
                wrapper.Instance is DataItem<T> dataItem &&
                dataItem.Value.Equals(item))
            {
                widget.Selected = i;
                return;
            }
        }
    }

    /// <summary>
    /// Populates the dropdown with grouped items.
    /// </summary>
    /// <param name="items">The items to populate the dropdown with.</param>
    public void PopulateGrouped(IEnumerable<T> items)
    {
        // Group the items, explicitly ordering them by group key.
        IEnumerable<IGrouping<string, T>> groups = items
            .GroupBy(groupKeySelector)
            .OrderBy(g => g.Key);

        // Create the flattened list with headers
        List<IDropDownGroupItem> groupedItems = [];
        foreach (IGrouping<string, T> group in groups)
        {
            // Add header
            if (alwaysShowHeaders || group.Count() > 1)
                groupedItems.Add(new GroupHeaderItem(group.Key));

            // Add items
            foreach (T item in group)
                groupedItems.Add(new DataItem<T>(item, itemRenderer, group.Count() > 1));
        }

        // Populate the dropdown
        Populate(groupedItems);
    }

    /// <inheritdoc />
    protected override Label CreateWidget()
    {
        return new Label() { Halign = Align.Start };
    }

    /// <inheritdoc />
    protected override void BindWidget(IDropDownGroupItem item, Label widget)
    {
        if (item is GroupHeaderItem)
        {
            widget.AddCssClass("dropdown-header");
            widget.SetMarkup($"<b>{item.GetDisplayText()}</b>");
            widget.MarginStart = 0;
        }
        else if (item is DataItem<T> data)
        {
            widget.RemoveCssClass("dropdown-header");
            widget.SetText(item.GetDisplayText());
            widget.MarginStart = data.IsGrouped ? 6 : 0;
        }
    }
}
