using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Views.Helpers;

using ListStore = Gio.ListStore;
using static GObject.Object;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A widget displaying a list of options with a drop-down menu. Also supports
/// a custom mapping of values to display strings.
/// </summary>
public abstract class DropDownView<T, TWidget> : ViewBase<DropDown> where TWidget : Widget
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
    /// Create a new <see cref="DropDownView{T, TWidget}"/> instance.
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
		factory.OnBind += OnBind;

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
    public void Populate(IEnumerable<T> values)
    {
        // Temporarily disconnect the notify signal to avoid signal handlers
        // being called while we're populating the dropdown.
        widget.OnNotify -= NotifyHandler;

        T? selected = Selection;

        // Remove all existing items from the model.
        model.RemoveAll();

        // Populate the model with the new values.
        foreach (T element in values)
        {
            DropdownEntry entry = new DropdownEntry(element);
            GenericGObject<DropdownEntry> wrapper = new GenericGObject<DropdownEntry>(entry);
            model.Append(wrapper);
        }

        // if (selected != null && values.Contains(selected))
        //     Select(selected);

        // Reconnect the notify signal.
        widget.OnNotify += NotifyHandler;
    }

    /// <summary>
    /// Get the element at a particular index.
    /// </summary>
    /// <param name="index">The index of the element to get.</param>
    /// <returns>The element at the specified index.</returns>
    /// <remarks>
    /// This method is made available to derived classes because the base class
    /// packs the actual elements inside wrapper objects. This method unwraps
    /// these and returns the object passed in by the derived class.
    /// </remarks>
    protected T? GetElement(uint index)
    {
        if (index >= model.GetNItems())
            return default;

        GenericGObject<DropdownEntry>? wrapper = model.GetObject(index) as GenericGObject<DropdownEntry>;
        if (wrapper != null && wrapper.Instance.Value != null)
            return wrapper.Instance.Value;
        return default;
    }

    /// <summary>
    /// Create a new widget to display a row in the dropdown.
    /// </summary>
    protected abstract TWidget CreateWidget();

    /// <summary>
    /// Bind a particular data row to a widget which renders that row.
    /// </summary>
    /// <param name="item">The data row to bind.</param>
    /// <param name="widget">The widget to bind to.</param>
    protected abstract void BindWidget(T item, TWidget widget);

    /// <summary>
    /// The bind callback function which binds a particular data row to a
    /// widget which renders that row.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnBind(SignalListItemFactory sender, SignalListItemFactory.BindSignalArgs args)
    {
        try
        {
            ListItem item = (ListItem)args.Object;
            GenericGObject<DropdownEntry>? wrapper = item.GetItem() as GenericGObject<DropdownEntry>;
            TWidget? widget = item.GetChild() as TWidget;
            if (widget != null && wrapper != null)
                BindWidget(wrapper.Instance.Value, widget);
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
            item.SetChild(CreateWidget());
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
        /// The value.
        /// </summary>
        public T Value { get; private init; }

        /// <summary>
        /// Create a new <see cref="DropdownEntry"/> instance.
        /// </summary>
        /// <param name="value">The value of the entry.</param>
        public DropdownEntry(T value) => Value = value;
    }
}
