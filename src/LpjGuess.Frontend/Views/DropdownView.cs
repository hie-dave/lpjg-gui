using GObject;
using Gtk;
using Gio;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Views.Helpers;

using ListStore = Gio.ListStore;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A widget displaying a list of options with a drop-down menu. Also supports
/// a custom mapping of values to display strings.
/// </summary>
public class DropdownView<T> : DropDown where T : class
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
            return (SelectedItem as GenericGObject<DropdownEntry>)?.Instance.Value;

        }
    }

    /// <summary>
    /// Create a new <see cref="DropdownView{T}"/> instance.
    /// </summary>
    public DropdownView() : base()
    {
		// model = new GenericListModel<GenericGObject<DropdownEntry>>();
        // Model = model;
        GObject.Type itemType = GenericGObject<DropdownEntry>.GetGType();
        nint hnd = Gio.Internal.ListStore.New(itemType);
        model = new ListStore(new Gio.Internal.ListStoreHandle(hnd, true));

		OnSelectionChanged = new Event<T>();
        OnNotify += NotifyHandler;
    }

    /// <summary>
    /// Populate the dropdown with the specified values.
    /// </summary>
    /// <param name="values">The values to populate the dropdown with.</param>
    /// <param name="renderer">A function which takes a value and returns the string to display.</param>
    public void Populate(IEnumerable<T> values, Func<T, string> renderer)
    {
        model.FreezeNotify();

        model.RemoveAll();

        foreach (T element in values)
        {
            DropdownEntry entry = new DropdownEntry(renderer(element), element);
            GenericGObject<DropdownEntry> wrapper = new GenericGObject<DropdownEntry>(entry);
            model.Append(wrapper);
        }

        model.ThawNotify();
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
			if (property == selectedItemProperty && SelectedItem is GenericGObject<DropdownEntry> wrapper)
            {
				OnSelectionChanged.Invoke(wrapper.Instance.Value);
            }
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
