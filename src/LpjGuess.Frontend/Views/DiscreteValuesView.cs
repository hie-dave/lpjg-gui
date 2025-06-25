using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for a discrete values generator.
/// </summary>
public class DiscreteValuesView : ViewBase<Box>, IDiscreteValuesView
{
    /// <summary>
    /// Wrapper class for the hboxes for the individual values.
    /// </summary>
    private class ValueContainer : ViewBase<Box>
    {
        /// <summary>
        /// The entry widget allowing the user to edit the value.
        /// </summary>
        private readonly Entry entry;

        /// <summary>
        /// The button to remove this value.
        /// </summary>
        private readonly Button removeButton;

        /// <summary>
        /// The event raised when the value changes.
        /// </summary>
        public Event OnChanged { get; private init; }

        /// <summary>
        /// The event raised when the value is removed.
        /// </summary>
        public Event OnRemove { get; private init; }

        /// <summary>
        /// The current value.
        /// </summary>
        public string Text => entry.GetText();

        /// <summary>
        /// Create a new <see cref="ValueContainer"/> instance.
        /// </summary>
        /// <param name="value">The initial value.</param>
        public ValueContainer(string value) : base(Box.New(Orientation.Horizontal, spacing))
        {
            OnChanged = new Event();
            OnRemove = new Event();

            entry = new Entry();
            entry.SetText(value);
            entry.OnActivate += OnValueChanged;
            entry.Halign = Align.Fill;
            entry.Hexpand = true;

            removeButton = Button.NewFromIconName(Icons.Delete);
            removeButton.AddCssClass(StyleClasses.DestructiveAction);
            removeButton.OnClicked += OnRemoveClicked;
            removeButton.Halign = Align.End;
            removeButton.Hexpand = false;

            widget.Append(entry);
            widget.Append(removeButton);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            entry.OnActivate -= OnValueChanged;
            removeButton.OnClicked -= OnRemoveClicked;
            OnChanged.Dispose();
            OnRemove.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Handle the user clicking the "Remove" button.
        /// </summary>
        /// <param name="sender">The button widget.</param>
        /// <param name="args">The event arguments.</param>
        private void OnRemoveClicked(Button sender, EventArgs args)
        {
            try
            {
                OnRemove.Invoke();
            }
            catch (Exception error)
            {
                MainView.Instance.ReportError(error);
            }
        }

        /// <summary>
        /// Handle a change to the value entry.
        /// </summary>
        /// <param name="sender">The entry widget.</param>
        /// <param name="args">The event arguments.</param>
        private void OnValueChanged(Entry sender, EventArgs args)
        {
            try
            {
                OnChanged.Invoke();
            }
            catch (Exception error)
            {
                MainView.Instance.ReportError(error);
            }
        }
    }

    /// <summary>
    /// The spacing between child widgets.
    /// </summary>
    private const int spacing = 6;

    /// <summary>
    /// The button to add a new value.
    /// </summary>
    private readonly Button addButton;

    /// <summary>
    /// The list of entries used to edit the values.
    /// </summary>
    private readonly List<ValueContainer> entries;

    /// <inheritdoc />
    public Event<IEnumerable<string>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event OnAddValue { get; private init; }

    /// <inheritdoc />
    public Event<int> OnRemoveValue { get; private init; }

    /// <summary>
    /// Create a new <see cref="DiscreteValuesView"/> instance.
    /// </summary>
    public DiscreteValuesView() : base(Box.New(Orientation.Vertical, spacing))
    {
        OnChanged = new Event<IEnumerable<string>>();
        OnAddValue = new Event();
        OnRemoveValue = new Event<int>();

        addButton = Button.NewWithLabel("Add Value");
        addButton.Hexpand = true;
        addButton.OnClicked += OnAddValueClicked;
        entries = new List<ValueContainer>();
        widget.Append(addButton);
    }

    /// <inheritdoc />
    public void Populate(IEnumerable<string> values)
    {
        Clear();
        foreach (string value in values)
        {
            ValueContainer container = new ValueContainer(value);
            container.OnChanged.ConnectTo(OnValueChanged);
            container.OnRemove.ConnectTo(() => OnRemove(container));
            widget.Append(container.GetWidget());
            entries.Add(container);
        }
        widget.Append(addButton);
    }

    /// <summary>
    /// Remove all widgets from the container box.
    /// </summary>
    private void Clear()
    {
        foreach (ValueContainer container in entries)
        {
            widget.Remove(container.GetWidget());
            container.Dispose();
        }
        entries.Clear();

        // Don't dispose of the "Add" button - we will reuse it.
        widget.Remove(addButton);
    }

    /// <summary>
    /// Handle a change to a value entry.
    /// </summary>
    private void OnValueChanged()
    {
        OnChanged.Invoke(entries.Select(e => e.Text).ToList());
    }

    /// <summary>
    /// Handle a request to remove a value.
    /// </summary>
    /// <param name="container">The container for the value to remove.</param>
    private void OnRemove(ValueContainer container)
    {
        OnRemoveValue.Invoke(entries.IndexOf(container));
    }

    /// <summary>
    /// Handle the user clicking the "Add Value" button.
    /// </summary>
    /// <param name="sender">The button widget.</param>
    /// <param name="args">The event arguments.</param>
    private void OnAddValueClicked(Button sender, EventArgs args)
    {
        try
        {
            OnAddValue.Invoke();
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
