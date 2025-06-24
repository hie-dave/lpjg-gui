using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view with a listbox widget allowing navigation between a list of other
/// widgets displayed in some other widget which must be implemented by the
/// derived class.
/// </summary>
public abstract class ListBoxNavigatorView : ViewBase<Box>
{
    /// <summary>
    /// A wrapper for a GtkListBoxRow which contains a GtkLabel and a GtkButton.
    /// </summary>
    private class RowWrapper : ViewBase<ListBoxRow>
    {
        /// <summary>
        /// The spacing between widgets in the row.
        /// </summary>
        private const int spacing = 6;

        /// <summary>
        /// The label containing the factor name.
        /// </summary>
        private readonly Label label;

        /// <summary>
        /// The button which deletes the factor.
        /// </summary>
        private readonly Button deleteButton;

        /// <summary>
        /// The view displaying the widget in the stack.
        /// </summary>
        public IView FactorView { get; }

        /// <summary>
        /// Event which is raised when the user wants to remove the factor.
        /// </summary>
        public Event OnRemove { get; private init; }

        /// <summary>
        /// Create a new <see cref="RowWrapper"/> instance.
        /// </summary>
        /// <param name="name">The name of the factor.</param>
        /// <param name="view">The view to add.</param>
        /// <param name="id">The ID of the row.</param>
        public RowWrapper(string name, IView view, Guid id) : base(new ListBoxRow())
        {
            OnRemove = new Event();
            FactorView = view;
            label = Label.New(name);
            label.Halign = Align.Start;
            label.Hexpand = true;

            deleteButton = Button.NewFromIconName(Icons.Delete);
            deleteButton.AddCssClass(StyleClasses.DestructiveAction);
            deleteButton.Name = name;
            deleteButton.OnClicked += OnDeleteFactor;

            Image icon = Image.NewFromIconName(Icons.GoNext);
            icon.Halign = Align.End;
            icon.Valign = Align.Center;

            Box rowBox = Box.New(Orientation.Horizontal, spacing);
            rowBox.Append(label);
            rowBox.Append(deleteButton);
            rowBox.Append(icon);

            // We can safely use the ID for the title as well, because the title is
            // not (necessarily) rendered anywhere in the UI.
            widget.Child = rowBox;
            widget.Name = id.ToString();

            // Note that the ListBox has Hexpand = false, so this will not cause the
            // row widgets to grow beyond the width of the sidebar.
            widget.Hexpand = true;
        }

        /// <summary>
        /// Change the name of the factor.
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name) => label.SetText(name);

        /// <inheritdoc />
        public override void Dispose()
        {
            deleteButton.OnClicked -= OnDeleteFactor;
            OnRemove.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Called when the user wants to delete a factor.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="args">Event data.</param>
        private void OnDeleteFactor(Button sender, EventArgs args)
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
    }

    /// <summary>
    /// The list box widget.
    /// </summary>
    private readonly ListBox listBox;

    /// <summary>
    /// The dictionary of child views. These are the individual views which are
    /// selectable via the listbox.
    /// </summary>
    private readonly Dictionary<Guid, Widget> children;

    /// <summary>
    /// The button which adds a new item.
    /// </summary>
    private readonly Button addButton;

    /// <summary>
    /// The container for the ListBox and the add button.
    /// </summary>
    protected readonly Box mainPage;

    /// <summary>
    /// The rows in the list box.
    /// </summary>
    private List<RowWrapper> rows;

    /// <summary>
    /// The event which is raised when the user wants to add a new child view.
    /// </summary>
    public Event OnAdd { get; private init; }

    /// <summary>
    /// The event which is raised when the user wants to remove a child view.
    /// </summary>
    public Event<string> OnRemove { get; private init; }

    /// <summary>
    /// The text to display on the add button.
    /// </summary>
    public string AddText
    {
        get => addButton.Label ?? string.Empty;
        set => addButton.Label = value;
    }

    /// <summary>
    /// Create a new <see cref="ListBoxNavigatorView"/> instance.
    /// </summary>
    public ListBoxNavigatorView() : base(new Box())
    {
        children = new Dictionary<Guid, Widget>();
        rows = new List<RowWrapper>();

        OnAdd = new Event();
        OnRemove = new Event<string>();

        listBox = new ListBox();
        listBox.Vexpand = true;
        listBox.AddCssClass("navigation-sidebar");

        addButton = Button.NewWithLabel("Add Item");
        mainPage = Box.New(Orientation.Vertical, 6);
        mainPage.Append(listBox);
        mainPage.Append(addButton);

        widget.SetOrientation(Orientation.Vertical);
        // Implementations must add content to the main widget.

        ConnectEvents();
    }

    /// <summary>
    /// Populate the view.
    /// </summary>
    /// <param name="views">The views to populate the view with.</param>
    public void Populate(IEnumerable<INamedView> views)
    {
        // Remove the existing contents of the listbox.
        listBox.RemoveAll();
        foreach (RowWrapper row in rows)
            row.Dispose();
        rows.Clear();

        // Populate the listbox with the given factor views.
        foreach (INamedView view in views)
            AddView(view.Name, view.View);
    }

    /// <summary>
    /// Rename a view.
    /// </summary>
    /// <param name="view">The view to rename.</param>
    /// <param name="name">The new name.</param>
    public void Rename(IView view, string name)
    {
        foreach (RowWrapper existingView in rows)
            if (existingView.FactorView == view)
                existingView.SetName(name);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnAdd.Dispose();
        OnRemove.Dispose();
        DisconnectEvents();
        rows.ForEach(r => r.Dispose());
        rows.Clear();
        base.Dispose();
    }

    /// <summary>
    /// Called when a child widget is selected.
    /// </summary>
    /// <param name="widget">The selected widget.</param>
    protected abstract void OnChildSelected(Widget widget);

    /// <summary>
    /// Add a child widget and return a Guid which will be associated with the
    /// widget.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    /// <returns>A Guid associated with the widget.</returns>
    protected abstract Widget AddChild(Widget widget);

    /// <summary>
    /// Add a view to the listbox and stack.
    /// </summary>
    /// <param name="name">The name of the view.</param>
    /// <param name="view">The view to add.</param>
    private void AddView(string name, IView view)
    {
        Widget child = AddChild(view.GetWidget());
        Guid id = Guid.NewGuid();
        children[id] = child;

        RowWrapper row = new RowWrapper(name, view, id);
        row.OnRemove.ConnectTo(() => OnRemove.Invoke(name));
        listBox.Append(row.GetWidget());
        rows.Add(row);
    }

    /// <summary>
    /// Connect all event handlers to gtk widgets.
    /// </summary>
    private void ConnectEvents()
    {
        listBox.OnRowActivated += OnListBoxRowActivated;
        addButton.OnClicked += OnAddClicked;
    }

    /// <summary>
    /// Disconnect all event handlers from gtk widgets.
    /// </summary>
    private void DisconnectEvents()
    {
        listBox.OnRowActivated -= OnListBoxRowActivated;
        addButton.OnClicked -= OnAddClicked;
    }

    /// <summary>
    /// Called when the user clicks the "Add" button.
    /// </summary>
    /// <param name="sender">The button.</param>
    /// <param name="args">Event data.</param>
    private void OnAddClicked(Button sender, EventArgs args)
    {
        try
        {
            OnAdd.Invoke();
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when a row in the listbox is activated.
    /// </summary>
    /// <param name="sender">The listbox.</param>
    /// <param name="args">The row-activated signal arguments.</param>
    private void OnListBoxRowActivated(ListBox sender, ListBox.RowActivatedSignalArgs args)
    {
        try
        {
            string? id = args.Row.Name;
            if (id == null)
                return;

            if (!Guid.TryParse(id, out Guid guid))
            {
                Console.WriteLine($"{nameof(ListBoxStackView)}: Invalid widget name in GtkListBox: {id}");
                return;
            }

            if (!children.TryGetValue(guid, out Widget? widget))
            {
                Console.WriteLine($"{nameof(ListBoxStackView)}: Unknown GUID: {guid}");
                return;
            }

            OnChildSelected(widget);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}

