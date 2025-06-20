using Gtk;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for a top-level factor generator.
/// </summary>
public class BlockFactorGeneratorView : ViewBase<ScrolledWindow>, IBlockFactorGeneratorView
{
    /// <summary>
    /// The container for the view.
    /// </summary>
    private readonly Box container;

    /// <summary>
    /// The entry containing the factor name.
    /// </summary>
    private readonly Entry nameEntry;

    /// <summary>
    /// The entry containing the block type.
    /// </summary>
    private readonly Entry blockTypeEntry;

    /// <summary>
    /// The entry containing the block name.
    /// </summary>
    private readonly Entry blockNameEntry;

    /// <summary>
    /// The list box containing the factor values.
    /// </summary>
    private readonly ListBox valuesListBox;

    /// <summary>
    /// The button to add a new factor value.
    /// </summary>
    private readonly Button addFactorButton;

    /// <summary>
    /// The grid housing the scalar inputs.
    /// </summary>
    private readonly Grid grid;

    /// <summary>
    /// Number of controls in the grid.
    /// </summary>
    private int ncontrols;

    /// <summary>
    /// The rows in the list box.
    /// </summary>
    private List<ListBoxRow> rows;

    /// <inheritdoc />
    public Event<IModelChange<BlockFactorGenerator>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event OnAddValue { get; private init; }

    /// <summary>
    /// Create a new <see cref="BlockFactorGeneratorView"/> instance.
    /// </summary>
    public BlockFactorGeneratorView() : base(new ScrolledWindow())
    {
        rows = [];
        OnChanged = new Event<IModelChange<BlockFactorGenerator>>();
        OnAddValue = new Event();

        // Create input widgets for the factor name, and block type/name.
        nameEntry = new Entry();
        blockTypeEntry = new Entry();
        blockNameEntry = new Entry();

        // Pack the scalar controls into the grid.
        grid = new Grid();
        grid.RowSpacing = grid.ColumnSpacing = 6;
        AddControl("Name", nameEntry);
        AddControl("Block Type", blockTypeEntry);
        AddControl("Block Name", blockNameEntry);

        // Create a list box to hold the factor values.
        valuesListBox = new ListBox();
        valuesListBox.Vexpand = true;

        // Create an "add value" button.
        addFactorButton = Button.NewWithLabel("Add Value");

        // Pack child widgets into the container.
        container = new Box();
        container.Append(grid);
        container.Append(valuesListBox);
        container.Append(addFactorButton);

        // Pack the container into the main widget.
        widget.Child = container;

        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(string name, string blockType, string blockName, List<string> values)
    {
        // Populate the scalar inputs.
        nameEntry.SetText(name);
        blockTypeEntry.SetText(blockType);
        blockNameEntry.SetText(blockName);

        // ListBoxes don't own their contents.
        valuesListBox.RemoveAll();
        foreach (ListBoxRow row in rows)
        {
            if (row.Child is Entry entry)
                entry.OnActivate -= OnFactorValueChanged;
            row.Dispose();
        }
        rows.Clear();

        // Populate the values listbox.
        foreach (string value in values)
        {
            Entry entry = new Entry();
            entry.SetText(value);
            entry.OnActivate += OnFactorValueChanged;

            ListBoxRow row = new ListBoxRow();
            row.Child = entry;
            valuesListBox.Append(row);
            rows.Add(row);
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        DisconnectEvents();
        base.Dispose();
    }

    /// <summary>
    /// Connect all events.
    /// </summary>
    private void ConnectEvents()
    {
        nameEntry.OnActivate += OnNameChanged;
        blockTypeEntry.OnActivate += OnBlockTypeChanged;
        blockNameEntry.OnActivate += OnBlockNameChanged;
        addFactorButton.OnClicked += OnAddFactorButtonClicked;
    }

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    private void DisconnectEvents()
    {
        nameEntry.OnActivate -= OnNameChanged;
        blockTypeEntry.OnActivate -= OnBlockTypeChanged;
        blockNameEntry.OnActivate -= OnBlockNameChanged;
        addFactorButton.OnClicked -= OnAddFactorButtonClicked;
    }

    /// <summary>
    /// Add a control to the grid.
    /// </summary>
    /// <param name="title">The title of the control.</param>
    /// <param name="widget">The widget to add.</param>
    private void AddControl(string title, Widget widget)
    {
        Label label = Label.New($"{title}:");
        grid.Attach(label, 0, ncontrols, 1, 1);
        grid.Attach(widget, 1, ncontrols, 1, 1);
        ncontrols++;
    }

    /// <summary>
    /// Called when the name entry is activated.
    /// </summary>
    /// <param name="sender">The name entry.</param>
    /// <param name="args">The event arguments.</param>
    private void OnNameChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<BlockFactorGenerator, string>(
                f => f.Name,
                (f, name) => f.Name = name,
                nameEntry.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the block name entry is activated.
    /// </summary>
    /// <param name="sender">The block name entry.</param>
    /// <param name="args">The event arguments.</param>
    private void OnBlockNameChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<BlockFactorGenerator, string>(
                f => f.BlockName,
                (f, name) => f.BlockName = name,
                blockNameEntry.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the block type entry is activated.
    /// </summary>
    /// <param name="sender">The block type entry.</param>
    /// <param name="args">The event arguments.</param>
    private void OnBlockTypeChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<BlockFactorGenerator, string>(
                f => f.BlockType,
                (f, type) => f.BlockType = type,
                blockTypeEntry.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user has changed a factor value.
    /// </summary>
    /// <param name="sender">The entry widget.</param>
    /// <param name="args">The event arguments.</param>
    private void OnFactorValueChanged(Entry sender, EventArgs args)
    {
        try
        {
            if (!(sender.Parent is ListBoxRow row))
                throw new InvalidOperationException($"Entry is not a child of a GtkListBoxRow");

            // TODO: is it safe to rely on indices like this?
            int index = rows.IndexOf(row);
            OnChanged.Invoke(new ModelChangeEventArgs<BlockFactorGenerator, string>(
                f => f.Values[index],
                (f, value) => f.Values[index] = value,
                sender.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user has clicked the "Add Factor" button.
    /// </summary>
    /// <param name="sender">The button widget.</param>
    /// <param name="args">The event arguments.</param>
    private void OnAddFactorButtonClicked(Button sender, EventArgs args)
    {
        try
        {
            // Using a separate event here - we wouldn't dare presume to tell
            // the presenter which value should be added!
            OnAddValue.Invoke();
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
