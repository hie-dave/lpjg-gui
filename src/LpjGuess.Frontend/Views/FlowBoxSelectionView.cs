using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for selecting one or more items from a list of items displayed in a
/// FlowBox.
/// </summary>
public class FlowBoxSelectionView : ViewBase<Box>, ISelectionView
{
    /// <summary>
    /// The horizontal spacing between "select all" and "clear" buttons.
    /// </summary>
    private const int buttonSpacing = 6;

    /// <summary>
    /// A dictionary mapping item names to check buttons.
    /// </summary>
    private readonly Dictionary<string, CheckButton> checkboxes = new();

    /// <summary>
    /// The flow box widget containing the checkboxes.
    /// </summary>
    private readonly FlowBox flowBox;

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
    /// Create a new <see cref="FlowBoxSelectionView"/> instance.
    /// </summary>
    public FlowBoxSelectionView() : base(new Box())
    {
        OnSelectionChanged = new Event<IEnumerable<string>>();

        selectAllButton = Button.New();
        selectAllButton.SetLabel("Select All");

        clearAllButton = Button.New();
        clearAllButton.SetLabel("Clear All");

        flowBox = FlowBox.New();
        flowBox.SetOrientation(Orientation.Horizontal);
        flowBox.RowSpacing = flowBox.ColumnSpacing = 6;
        // flowBox.Homogeneous = true;

        Box buttonBox = Box.New(Orientation.Horizontal, buttonSpacing);
        buttonBox.Append(selectAllButton);
        buttonBox.Append(clearAllButton);

        widget.SetOrientation(Orientation.Vertical);
        widget.Spacing = 6;
        widget.Append(flowBox);
        widget.Append(buttonBox);
        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(IEnumerable<string> items)
    {
        // Clear existing checkboxes.
        checkboxes.Values.ToList().ForEach(c => c.OnToggled -= OnCheckboxToggled);
        Widget? child;
        while ((child = flowBox.GetFirstChild()) != null)
        {
            flowBox.Remove(child);
            child.Dispose();
        }
        checkboxes.Clear();

        // Add checkboxes for each item.
        foreach (string item in items)
        {
            var checkbox = CheckButton.New();
            checkbox.SetLabel(item);
            checkbox.Active = false;
            checkbox.OnToggled += OnCheckboxToggled;

            checkboxes[item] = checkbox;
            flowBox.Append(checkbox);
        }
    }

    /// <inheritdoc />
    public void Select(IEnumerable<string> items)
    {
        foreach (string item in items)
        {
            if (!checkboxes.TryGetValue(item, out CheckButton? checkbox))
                throw new ArgumentException($"Item {item} not found.");

            checkbox.OnToggled -= OnCheckboxToggled;
            checkbox.Active = true;
            checkbox.OnToggled += OnCheckboxToggled;
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> GetSelectedItems()
    {
        return checkboxes
            .Where(kvp => kvp.Value.Active)
            .Select(kvp => kvp.Key);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        DisconnectEvents();
        checkboxes.Values.ToList().ForEach(c => c.OnToggled -= OnCheckboxToggled);
        checkboxes.Clear();
        OnSelectionChanged.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Connects the event handlers.
    /// </summary>
    private void ConnectEvents()
    {
        selectAllButton.OnClicked += OnSelectAllClicked;
        clearAllButton.OnClicked += OnClearAllClicked;
    }

    /// <summary>
    /// Disconnects the event handlers.
    /// </summary>
    private void DisconnectEvents()
    {
        selectAllButton.OnClicked -= OnSelectAllClicked;
        clearAllButton.OnClicked -= OnClearAllClicked;
    }

    /// <summary>
    /// Called when the user clicks the "clear all" button.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="args">The event arguments.</param>
    private void OnClearAllClicked(Button sender, EventArgs args)
    {
        try
        {
            Select([]);
            OnSelectionChanged.Invoke([]);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user clicks the "select all" button.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="args">The event arguments.</param>
    private void OnSelectAllClicked(Button sender, EventArgs args)
    {
        try
        {
            Select(checkboxes.Keys);
            OnSelectionChanged.Invoke(checkboxes.Keys);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user has toggled a checkbox.
    /// </summary>
    /// <param name="sender">The checkbox that was toggled.</param>
    /// <param name="args">The event arguments.</param>
    private void OnCheckboxToggled(CheckButton sender, EventArgs args)
    {
        try
        {
            OnSelectionChanged.Invoke(GetSelectedItems());
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
