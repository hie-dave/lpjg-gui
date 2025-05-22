using GObject;
using Gtk;
using Gio;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Views.Helpers;

using ListStore = Gio.ListStore;
using static GObject.Object;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A widget displaying a list of options with a drop-down menu. Also supports
/// a custom mapping of values to display strings.
/// </summary>
public class DropDownView<T> : ViewBase<DropDown>
{
	/// <summary>
	/// Name of the property corresponding to the selected item in a dropdown.
	/// </summary>
	private const string selectedItemProperty = "selected-item";

    /// <summary>
    /// The model containing the data.
    /// </summary>
    private readonly ListStore model;

    /// <summary>
    /// Called when the dropdown selection is changed by the user. Event
    /// parameter is the newly-selected value.
    /// </summary>
    public Event<T> OnSelectionChanged { get; private init; }

    /// <summary>
    /// Get the currently selected item.
    /// </summary>
    public T? Selection
    {
        get
        {
            if (widget.SelectedItem == null)
                // This apparently returns null. The null keyword will return
                // a reference type, and we can't guarantee here that T is a
                // reference type.
                return default;

            // The only objects we pack into the model are
            // GenericGObject<DropdownEntry>, so this should be a safe cast.
            return ((GenericGObject<DropdownEntry>)widget.SelectedItem).Instance.Value;
        }
    }

    /// <summary>
    /// Create a new <see cref="DropDownView{T}"/> instance.
    /// </summary>
    public DropDownView() : base(new DropDown())
    {
		// model = new GenericListModel<GenericGObject<DropdownEntry>>();
        // Model = model;
        GObject.Type itemType = GenericGObject<DropdownEntry>.GetGType();
        nint hnd = Gio.Internal.ListStore.New(itemType);
        model = new ListStore(new Gio.Internal.ListStoreHandle(hnd, true));

		SignalListItemFactory factory = SignalListItemFactory.New();
		factory.OnSetup += OnSetup;
		factory.OnBind += (_, args) => OnBind<Label>(args, (l, row) => l.SetText(row.Name));

        widget.Factory = factory;
        widget.Model = model;

		OnSelectionChanged = new Event<T>();
        widget.OnNotify += NotifyHandler;
    }

    /// <summary>
    /// Select a particular element in the dropdown.
    /// </summary>
    /// <param name="element">The element to select.</param>
    public void Select(T element)
    {
        // Temporarily disconnect the notify signal - we only want to receive
        // notifications when the user changes the selection, not when we're
        // setting it programmatically.
        widget.OnNotify -= NotifyHandler;

        try
        {
            for (uint i = 0; i < model.GetNItems(); i++)
            {
                GenericGObject<DropdownEntry>? wrapper = model.GetObject(i) as GenericGObject<DropdownEntry>;
                if (wrapper != null && wrapper.Instance.Value != null && wrapper.Instance.Value.Equals(element))
                {
                    widget.SetSelected(i);
                    return;
                }
            }
        }
        finally
        {
            // Reconnect the notify signal.
            widget.OnNotify += NotifyHandler;
        }
    }

    /// <summary>
    /// Populate the dropdown with the specified values.
    /// </summary>
    /// <param name="values">The values to populate the dropdown with.</param>
    /// <param name="renderer">A function which takes a value and returns the string to display.</param>
    public void Populate(IEnumerable<T> values, Func<T, string> renderer)
    {
        // Temporarily disconnect the notify signal to avoid signal handlers
        // being called while we're populating the dropdown.
        widget.OnNotify -= NotifyHandler;

        // Remove all existing items from the model.
        model.RemoveAll();

        // Populate the model with the new values.
        foreach (T element in values)
        {
            DropdownEntry entry = new DropdownEntry(renderer(element), element);
            GenericGObject<DropdownEntry> wrapper = new GenericGObject<DropdownEntry>(entry);
            model.Append(wrapper);
        }

        // Reconnect the notify signal.
        widget.OnNotify += NotifyHandler;
    }

    /// <summary>
    /// The bind callback function which binds a particular data row to a
    /// widget which renders that row.
    /// </summary>
    /// <typeparam name="TWidget">The widget type of the column.</typeparam>
    /// <param name="args">Sender object.</param>
    /// <param name="bind">Event data.</param>
    private void OnBind<TWidget>(SignalListItemFactory.BindSignalArgs args, Action<TWidget, DropdownEntry> bind)
        where TWidget : Widget
    {
        try
        {
            ListItem item = (ListItem)args.Object;
            GenericGObject<DropdownEntry>? wrapper = item.GetItem() as GenericGObject<DropdownEntry>;
            TWidget? widget = item.GetChild() as TWidget;
            if (widget != null && wrapper != null)
                bind(widget, wrapper.Instance);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// The factory setup callback. This creates the widgets used to display
    /// data in a particular column.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnSetup(SignalListItemFactory sender, SignalListItemFactory.SetupSignalArgs args)
    {
        try
        {
            ListItem item = (ListItem)args.Object;
            item.SetChild(new Label() { Halign = Align.Start });
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Callback for the notify signal on the dropdown widget.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void NotifyHandler(GObject.Object sender, NotifySignalArgs args)
    {
        try
		{
			string property = args.Pspec.GetName();
			if (property == selectedItemProperty && widget.SelectedItem is GenericGObject<DropdownEntry> wrapper)
				OnSelectionChanged.Invoke(wrapper.Instance.Value);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
    }

    /// <summary>
    /// Represents an entry in the dropdown, which can (potentially) have a
    /// different string displayed (name) to the actual value passed in by the
    /// caller.
    /// </summary>
    private class DropdownEntry
    {
        /// <summary>
        /// Display name of this entry in the dropdown.
        /// </summary>
        public string Name { get; private init; }
        /// <summary>
        /// The value.
        /// </summary>
        public T Value { get; private init; }

        /// <summary>
        /// Create a new <see cref="DropdownEntry"/> instance.
        /// </summary>
        /// <param name="name">The display name of the entry.</param>
        /// <param name="value">The value of the entry.</param>
        public DropdownEntry(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }
}
