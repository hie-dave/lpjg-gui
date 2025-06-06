using Gtk;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for selecting columns to plot.
/// </summary>
public class ColumnSelectionView : ViewBase<Button>
{
    /// <summary>
    /// A box containing the checkbox widgets.
    /// </summary>
    private readonly Box container;

    /// <summary>
    /// The popover that contains the checkbox widgets.
    /// </summary>
    private readonly Popover selectionPopover;

    /// <summary>
    /// A dictionary mapping column names to check buttons.
    /// </summary>
    private readonly Dictionary<string, CheckButton> columnCheckboxes = new();

    /// <summary>
    /// The button that selects all checkboxes.
    /// </summary>
    private readonly Button selectAllButton;

    /// <summary>
    /// The button that clears all checkboxes.
    /// </summary>
    private readonly Button clearAllButton;

    /// <summary>
    /// The event that is raised when the selection changes.
    /// </summary>
    public Event<IEnumerable<string>> OnSelectionChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="ColumnSelectionView"/> instance.
    /// </summary>
    public ColumnSelectionView() : base(new Button())
    {
        OnSelectionChanged = new Event<IEnumerable<string>>();

        // Create the dropdown button
        widget.SetLabel("Select Y-axis columns...");
        widget.Hexpand = true;

        // Create the popover
        selectionPopover = Popover.New();
        selectionPopover.SetParent(widget);

        // Create container for checkboxes
        var popoverContent = Box.New(Orientation.Vertical, 6);
        popoverContent.MarginStart = 12;
        popoverContent.MarginEnd = 12;
        popoverContent.MarginTop = 12;
        popoverContent.MarginBottom = 12;

        // Label at the top
        var label = Label.New("Select columns to plot:");
        popoverContent.Append(label);

        // Scrolled window for checkboxes
        var scrolled = ScrolledWindow.New();
        scrolled.SetPolicy(PolicyType.Never, PolicyType.Automatic);
        scrolled.SetSizeRequest(-1, 200); // Height limit

        container = new Box();
        scrolled.SetChild(container);
        popoverContent.Append(scrolled);

        // Buttons at the bottom
        var buttonBox = Box.New(Orientation.Horizontal, 6);
        buttonBox.Homogeneous = true;

        selectAllButton = Button.New();
        selectAllButton.SetLabel("Select All");
        selectAllButton.OnClicked += OnSelectAllClicked;

        clearAllButton = Button.New();
        clearAllButton.SetLabel("Clear All");
        clearAllButton.OnClicked += OnClearAllClicked;

        buttonBox.Append(selectAllButton);
        buttonBox.Append(clearAllButton);
        popoverContent.Append(buttonBox);

        selectionPopover.SetChild(popoverContent);

        // Connect events
        widget.OnClicked += (sender, args) => selectionPopover.Popup();
    }

    /// <summary>
    /// Populates the view with the specified columns.
    /// </summary>
    /// <param name="columns">The columns to populate the view with.</param>
    public void Populate(IEnumerable<string> columns)
    {
        // Clear existing checkboxes
        Widget? child;
        while ( (child = container.GetFirstChild()) != null)
            container.Remove(child);
        columnCheckboxes.Clear();

        // Add checkboxes for each column.
        foreach (string column in columns)
        {
            var checkbox = CheckButton.New();
            checkbox.SetLabel(column);
            checkbox.Active = false;
            checkbox.OnToggled += OnCheckboxToggled;

            columnCheckboxes[column] = checkbox;
            container.Append(checkbox);
        }

        UpdateButtonLabel();
    }

    /// <summary>
    /// Selects the specified columns.
    /// </summary>
    /// <param name="columns">The columns to select.</param>
    public void Select(IEnumerable<string> columns)
    {
        foreach (string column in columns)
        {
            if (!columnCheckboxes.TryGetValue(column, out CheckButton? checkbox))
                throw new ArgumentException($"Column {column} not found.");
            checkbox.Active = true;
        }

        UpdateButtonLabel();
    }

    /// <summary>
    /// Gets the currently selected columns.
    /// </summary>
    /// <returns>The currently selected columns.</returns>
    public IEnumerable<string> GetSelectedColumns()
    {
        return columnCheckboxes
            .Where(kvp => kvp.Value.Active)
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Handles the checkbox toggled event.
    /// </summary>
    /// <param name="sender">The checkbox that was toggled.</param>
    /// <param name="args">The event arguments.</param>
    private void OnCheckboxToggled(CheckButton sender, EventArgs args)
    {
        try
        {
            UpdateButtonLabel();
            OnSelectionChanged.Invoke(GetSelectedColumns());
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Handles the select all button clicked event.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="args">The event arguments.</param>
    private void OnSelectAllClicked(Button sender, EventArgs args)
    {
        try
        {
            foreach (var checkbox in columnCheckboxes.Values)
                checkbox.Active = true;

            UpdateButtonLabel();
            OnSelectionChanged.Invoke(GetSelectedColumns());
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Handles the clear all button clicked event.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="args">The event arguments.</param>
    private void OnClearAllClicked(Button sender, EventArgs args)
    {
        try
        {
            foreach (var checkbox in columnCheckboxes.Values)
                checkbox.Active = false;

            UpdateButtonLabel();
            OnSelectionChanged.Invoke(GetSelectedColumns());
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Updates the button label based on the currently selected columns.
    /// </summary>
    private void UpdateButtonLabel()
    {
        var selectedCount = GetSelectedColumns().Count();

        if (selectedCount == 0)
            widget.SetLabel("Select Y-axis columns...");
        else if (selectedCount == 1)
            widget.SetLabel(GetSelectedColumns().First());
        else
            widget.SetLabel($"{selectedCount} columns selected");
    }
}
