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
public class TopLevelFactorGeneratorView : ViewBase<ScrolledWindow>, ITopLevelFactorGeneratorView
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
    /// The list box containing the factor values.
    /// </summary>
    private readonly ListBox valuesListBox;

    /// <summary>
    /// The button to add a new factor value.
    /// </summary>
    private readonly Button addFactorButton;

    /// <summary>
    /// The rows in the list box.
    /// </summary>
    private List<ListBoxRow> rows;

    /// <inheritdoc />
    public Event<IModelChange<TopLevelFactorGenerator>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event OnAddValue { get; private init; }

    /// <summary>
    /// Create a new <see cref="TopLevelFactorGeneratorView"/> instance.
    /// </summary>
    public TopLevelFactorGeneratorView() : base(new ScrolledWindow())
    {
        rows = [];
        OnChanged = new Event<IModelChange<TopLevelFactorGenerator>>();
        OnAddValue = new Event();

        // Create an input widget for the factor name.
        Label entryLabel = Label.New("Name:");
        nameEntry = new Entry();
        Box entryBox = Box.New(Orientation.Horizontal, 6);
        entryBox.Append(entryLabel);
        entryBox.Append(nameEntry);

        // Create a list box to hold the factor values.
        valuesListBox = new ListBox();
        valuesListBox.Vexpand = true;

        // Create an "add value" button.
        addFactorButton = Button.NewWithLabel("Add Value");

        // Pack child widgets into the container.
        container = Box.New(Orientation.Vertical, 6);
        container.Append(entryBox);
        container.Append(valuesListBox);
        container.Append(addFactorButton);

        // Pack the container into the main widget.
        widget.Child = container;
        widget.Vexpand = true;

        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(string name, List<string> values)
    {
        // Populate the name entry.
        nameEntry.SetText(name);

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
            entry.Hexpand = true;

            ListBoxRow row = new ListBoxRow();
            row.Child = entry;
            row.Vexpand = true;
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
        addFactorButton.OnClicked += OnAddFactorButtonClicked;
    }

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    private void DisconnectEvents()
    {
        nameEntry.OnActivate -= OnNameChanged;
        addFactorButton.OnClicked -= OnAddFactorButtonClicked;
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
            OnChanged.Invoke(new ModelChangeEventArgs<TopLevelFactorGenerator, string>(
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
            OnChanged.Invoke(new ModelChangeEventArgs<TopLevelFactorGenerator, string>(
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
