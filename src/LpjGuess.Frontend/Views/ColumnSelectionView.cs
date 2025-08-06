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
    /// The label at the top of the popover.
    /// </summary>
    private readonly Label label;

    /// <summary>
    /// The columns which were selected by the last call to <see cref="Select"/>.
    /// </summary>
    private List<string> selectedColumns;

    /// <summary>
    /// The text on the button when no items are selected.
    /// </summary>
    private string buttonTextNoSelection = "Select Y-axis columns...";

    /// <summary>
    /// The text on the button when multiple items are selected.
    /// </summary>
    private Func<int, string> buttonTextMultipleSelected = count => $"{count} columns selected";

    /// <summary>
    /// The event that is raised when the selection changes.
    /// </summary>
    public Event<IEnumerable<string>> OnSelectionChanged { get; private init; }

    /// <summary>
    /// Gets or sets the text on the button when no items are selected.
    /// </summary>
    public string ButtonTextNoSelection
    {
        get => buttonTextNoSelection;
        set
        {
            buttonTextNoSelection = value;
            UpdateButtonLabel();
        }
    }

    /// <summary>
    /// Gets or sets the function that returns the text on the button when multiple items are selected.
    /// </summary>
    public Func<int, string> ButtonTextMultipleSelected
    {
        get => buttonTextMultipleSelected;
        set
        {
            buttonTextMultipleSelected = value;
            UpdateButtonLabel();
        }
    }

    /// <summary>
    /// Create a new <see cref="ColumnSelectionView"/> instance.
    /// </summary>
    public ColumnSelectionView() : base(new Button())
    {
        selectedColumns = [];
        OnSelectionChanged = new Event<IEnumerable<string>>();

        // Create the dropdown button
        widget.SetLabel(ButtonTextNoSelection);
        widget.Hexpand = true;

        // Create the popover
        selectionPopover = Popover.New();
        selectionPopover.SetParent(widget);
        selectionPopover.OnClosed += OnPopoverClosed;

        // Create container for checkboxes
        var popoverContent = Box.New(Orientation.Vertical, 6);
        popoverContent.MarginStart = 12;
        popoverContent.MarginEnd = 12;
        popoverContent.MarginTop = 12;
        popoverContent.MarginBottom = 12;

        // Label at the top
        label = Label.New("Select columns to plot:");
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
        widget.OnClicked += OnClicked;
    }

    /// <summary>
    /// Populates the view with the specified columns.
    /// </summary>
    /// <param name="columns">The columns to populate the view with.</param>
    public void Populate(IEnumerable<string> columns)
    {
        // Clear existing checkboxes
        Widget? child;
        while ((child = container.GetFirstChild()) != null)
        {
            container.Remove(child);
            child.Dispose();
        }
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
        selectedColumns = columns.ToList();
        foreach (string column in columns)
        {
            if (!columnCheckboxes.TryGetValue(column, out CheckButton? checkbox))
                throw new ArgumentException($"Column {column} not found.");

            checkbox.OnToggled -= OnCheckboxToggled;
            checkbox.Active = true;
            checkbox.OnToggled += OnCheckboxToggled;
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
    /// Sets the text displayed on the label at the top of the popover.
    /// </summary>
    /// <param name="text">The text to display on the label.</param>
    public void SetLabelText(string text)
    {
        label.SetLabel(text);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        widget.OnClicked -= OnClicked;
        selectAllButton.OnClicked -= OnSelectAllClicked;
        clearAllButton.OnClicked -= OnClearAllClicked;
        selectionPopover.OnClosed -= OnPopoverClosed;

        foreach (CheckButton checkbox in columnCheckboxes.Values)
            checkbox.OnToggled -= OnCheckboxToggled;
        selectionPopover.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Updates the button label based on the currently selected columns.
    /// </summary>
    private void UpdateButtonLabel()
    {
        var selectedCount = GetSelectedColumns().Count();

        if (selectedCount == 0)
            widget.SetLabel(buttonTextNoSelection);
        else if (selectedCount == 1)
            widget.SetLabel(GetSelectedColumns().First());
        else
            widget.SetLabel(buttonTextMultipleSelected(selectedCount));
    }

    /// <summary>
    /// Checks if there are any pending changes.
    /// </summary>
    /// <returns>True if there are pending changes, false otherwise.</returns>
    private bool PendingChanges()
    {
        return !selectedColumns.OrderBy(c => c)
            .SequenceEqual(GetSelectedColumns().OrderBy(c => c));
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
            // OnSelectionChanged.Invoke(GetSelectedColumns());
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
    /// Handles the popover closed event.
    /// </summary>
    /// <param name="sender">The popover that was closed.</param>
    /// <param name="args">The event arguments.</param>
    private void OnPopoverClosed(Popover sender, EventArgs args)
    {
        try
        {
            if (PendingChanges())
                OnSelectionChanged.Invoke(GetSelectedColumns());
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Handles the button clicked event.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="args">The event arguments.</param>
    private void OnClicked(Button sender, EventArgs args)
    {
        try
        {
            selectionPopover.Popup();
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
