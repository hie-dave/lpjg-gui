using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays multiple child views in a stack, which is navigatable
/// via a ListBox widget.
/// </summary>
/// <remarks>
/// At all times, the list box or one child of the stack is visible. The list
/// box contains one element for each child view in the stack, and clicking on
/// that row will display the corresponding child view, along with a back button
/// which returns the user to the list box.
///
/// The class also supports dynamic addition/removal of child views.
/// </remarks>
public class ListBoxStackView : ViewBase<Box>
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
    /// The stack widget.
    /// </summary>
    private readonly Stack stack;

    /// <summary>
    /// The dictionary of factor views. These are the pages in the stack, and
    /// the keys are the IDs of the views - the names of the Gtk widgets.
    /// </summary>
    private readonly Dictionary<Guid, Widget> stackChildren;

    /// <summary>
    /// The button which adds a new item.
    /// </summary>
    private readonly Button addButton;

    /// <summary>
    /// The container for the ListBox and the add button, which is used as the
    /// "main" page of the stack.
    /// </summary>
    private readonly Box mainPage;

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
    /// Create a new <see cref="ListBoxStackView"/> instance.
    /// </summary>
    public ListBoxStackView() : base(new Box())
    {
        stackChildren = new Dictionary<Guid, Widget>();
        rows = new List<RowWrapper>();

        OnAdd = new Event();
        OnRemove = new Event<string>();

        listBox = new ListBox();
        listBox.Vexpand = true;
        listBox.AddCssClass("navigation-sidebar");

        stack = new Stack();
        stack.Vexpand = true;

        addButton = Button.NewWithLabel("Add Item");
        mainPage = Box.New(Orientation.Vertical, 6);
        mainPage.Append(listBox);
        mainPage.Append(addButton);

        // ListBox goes into the stack, stack goes into the main widget.
        AddToStack(mainPage);
        stack.TransitionType = StackTransitionType.SlideLeftRight;
        stack.VisibleChild = mainPage;

        widget.SetOrientation(Orientation.Vertical);
        widget.Append(stack);

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

    /// <summary>
    /// Add a view to the listbox and stack.
    /// </summary>
    /// <param name="name">The name of the view.</param>
    /// <param name="view">The view to add.</param>
    private void AddView(string name, IView view)
    {
        // Create the widget which goes into the stack. This will be a VBox with
        // a back button at the top, and the widget beneath it.
        Button button = Button.NewFromIconName(Icons.GoPrevious);
        button.Halign = Align.Start;
        button.OnClicked += OnShowListBox;

        Box container = Box.New(Orientation.Vertical, 6);
        container.Append(button);
        container.Append(view.GetWidget());

        Guid id = AddToStack(container);

        RowWrapper row = new RowWrapper(name, view, id);
        row.OnRemove.ConnectTo(() => OnRemove.Invoke(name));
        listBox.Append(row.GetWidget());
        rows.Add(row);
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
    /// Add a widget to the stack.
    /// </summary>
    /// <param name="widget">The widget to be added.</param>
    private Guid AddToStack(Widget widget)
    {
        Guid id = Guid.NewGuid();
        widget.Name = id.ToString();
        stack.AddChild(widget);
        stackChildren[id] = widget;
        return id;
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

            if (!stackChildren.TryGetValue(guid, out Widget? widget))
            {
                Console.WriteLine($"{nameof(ListBoxStackView)}: Unknown GUID: {guid}");
                return;
            }

            stack.VisibleChild = widget;
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user clicks the back button on one of the stack pages to
    /// go back to the main listbox.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnShowListBox(Button sender, EventArgs args)
    {
        try
        {
            stack.VisibleChild = mainPage;
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
