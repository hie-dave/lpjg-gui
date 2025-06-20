using Gtk;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays a factorial.
/// </summary>
public class FactorialView : ViewBase<Box>, IFactorialView
{
    /// <summary>
    /// Widget spacing.
    /// </summary>
    private readonly int spacing = 6;

    /// <summary>
    /// The grid which displays the factorial properties.
    /// </summary>
    private readonly Grid grid;

    /// <summary>
    /// The switch which controls whether to generate a full factorial.
    /// </summary>
    private readonly Switch fullFactorialSwitch;

    /// <summary>
    /// The list box which displays the factors.
    /// </summary>
    private readonly ListBox factorsListBox;

    /// <summary>
    /// The button which adds a new factor.
    /// </summary>
    private readonly Button addFactorButton;

    /// <summary>
    /// The stack widget which houses the factor views.
    /// </summary>
    private readonly Stack stack;

    /// <summary>
    /// The dictionary of factor views. These are the pages in the stack, and
    /// the keys are the IDs of the views - the names of the Gtk widgets.
    /// </summary>
    private readonly Dictionary<Guid, Widget> factorViews;

    /// <summary>
    /// The rows in the list box.
    /// </summary>
    private List<ListBoxRow> rows;

    /// <summary>
    /// Number of rows currently in the grid.
    /// </summary>
    private int nrow;

    /// <inheritdoc />
    public Event<IModelChange<FactorialGenerator>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event OnAddFactor { get; private init; }

    /// <inheritdoc />
    public Event<string> OnRemoveFactor { get; private init; }

    /// <summary>
    /// Create a new <see cref="FactorialView"/> instance.
    /// </summary>
    public FactorialView() : base(new Box())
    {
        nrow = 0;
        OnChanged = new Event<IModelChange<FactorialGenerator>>();
        OnAddFactor = new Event();
        OnRemoveFactor = new Event<string>();
        rows = [];
        factorViews = [];

        // Configure container.
        widget.SetOrientation(Orientation.Vertical);
        widget.Spacing = spacing;

        // Initialise and configure child widgets.
        grid = new Grid();
        grid.RowSpacing = spacing;
        grid.ColumnSpacing = spacing;

        fullFactorialSwitch = new Switch();

        Label factorsLabel = new Label();
        factorsLabel.Halign = Align.Start;
        factorsLabel.SetText("Factors");
        factorsLabel.AddCssClass(StyleClasses.Heading);

        factorsListBox = new ListBox();
        factorsListBox.Vexpand = true;
        factorsListBox.OnRowActivated += OnListBoxRowActivated;
        factorsListBox.AddCssClass("navigation-sidebar");

        // Configure the stack.
        stack = new Stack();
        stack.Vexpand = true;
        AddToStack(factorsListBox);
        stack.TransitionType = StackTransitionType.SlideLeft;
        stack.VisibleChild = factorsListBox;

        addFactorButton = Button.NewWithLabel("Add Factor");
        addFactorButton.AddCssClass(StyleClasses.SuggestedAction);
        addFactorButton.Valign = Align.End;

        // Pack controls into the grid.
        AddControl("Full Factorial", fullFactorialSwitch);

        // Pack child widgets into the container.
        widget.Append(grid);
        widget.Append(factorsLabel);
        widget.Append(stack);
        widget.Append(addFactorButton);

        // Connect event handlers.
        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(bool fullFactorial, IEnumerable<INamedView> factorViews)
    {
        fullFactorialSwitch.SetActive(fullFactorial);

        // Remove the existing contents of the listbox.
        factorsListBox.RemoveAll();
        foreach (ListBoxRow row in rows)
            row.Dispose();
        rows.Clear();

        // Populate the listbox with the given factor views.
        foreach (INamedView view in factorViews)
            AddFactorView(view.Name, view.View.GetWidget());
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        DisconnectEvents();
        base.Dispose();
    }

    /// <summary>
    /// Add a row to the list box for the given widget.
    /// </summary>
    /// <param name="name">The name of the factor.</param>
    /// <param name="widget">The widget to add.</param>
    private void AddFactorView(string name, Widget widget)
    {
        Button button = Button.NewFromIconName(Icons.GoPrevious);
        button.Halign = Align.Start;
        button.OnActivate += OnHideFactorView;
        Box hbox = Box.New(Orientation.Horizontal, 0);
        hbox.Append(button);

        Box container = Box.New(Orientation.Vertical, spacing);
        container.Append(hbox);
        container.Append(widget);

        Guid id = AddToStack(container);

        Label label = Label.New(name);
        label.Hexpand = true;

        Button deleteButton = Button.NewFromIconName(Icons.Delete);
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
        ListBoxRow row = new ListBoxRow();
        row.Child = rowBox;
        row.Name = id.ToString();

        // Note that the ListBox has Hexpand = false, so this will not cause the
        // row widgets to grow beyond the width of the sidebar.
        row.Hexpand = true;
        factorsListBox.Append(row);
        rows.Add(row);
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
            string? name = sender.Name;
            if (name == null)
                return;

            OnRemoveFactor.Invoke(name);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Add a control to the grid.
    /// </summary>
    /// <param name="title">The title of the control.</param>
    /// <param name="widget">The widget to add.</param>
    private void AddControl(string title, Widget widget)
    {
        Label label = Label.New($"{title}:");
        label.Halign = Align.Start;
        grid.Attach(label, 0, nrow, 1, 1);
        grid.Attach(widget, 1, nrow, 1, 1);
        nrow++;
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
        factorViews[id] = widget;
        return id;
    }

    /// <summary>
    /// Connect all events.
    /// </summary>
    private void ConnectEvents()
    {
        fullFactorialSwitch.OnActivate += OnFullFactorialChanged;
        addFactorButton.OnClicked += OnAddFactorButtonClicked;
    }

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    private void DisconnectEvents()
    {
        fullFactorialSwitch.OnActivate -= OnFullFactorialChanged;
        addFactorButton.OnClicked -= OnAddFactorButtonClicked;
    }

    /// <summary>
    /// Called when the full factorial switch is toggled by the user.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnFullFactorialChanged(object sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<FactorialGenerator, bool>(
                f => f.FullFactorial,
                (f, fullFactorial) => f.FullFactorial = fullFactorial,
                fullFactorialSwitch.GetActive()
            ));
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
                Console.WriteLine($"Invalid GUID: {id}");
                return;
            }

            if (!factorViews.TryGetValue(guid, out Widget? widget))
            {
                Console.WriteLine($"Invalid GUID: {id}");
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
    /// Called when the add factor button is clicked.
    /// </summary>
    /// <param name="sender">The button.</param>
    /// <param name="args">The event arguments.</param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnAddFactorButtonClicked(Button sender, EventArgs args)
    {
        try
        {
            OnAddFactor.Invoke();
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Hide the currently-displayed factor view and show the list of factors in
    /// its place.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnHideFactorView(Button sender, EventArgs args)
    {
        try
        {
            stack.VisibleChild = factorsListBox;
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
