using Gtk;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A StackSidebar which gives the controller more control over the sidebar
/// widgets.
/// </summary>
public class DynamicStackSidebar : Paned
{
    private class StackEntry
    {
        public string Name { get; private init; }
        public Widget Page { get; private init; }
        public Widget SidebarWidget { get; private init; }

        public StackEntry(string name, Widget page, Widget sidebarWidget)
        {
            Name = name;
            Page = page;
            SidebarWidget = sidebarWidget;
        }
    }

    /// <summary>
    /// The stack which the sidebar manages.
    /// </summary>
    private Stack stack;

    /// <summary>
    /// The sidebar which allows navigation between the widgets of the stack.
    /// </summary>
    private readonly ListBox sidebar;

    private readonly List<StackEntry> pages;

    /// <summary>
    /// Called when the user selects a page.
    /// </summary>
    public Event<string> OnPageSelected { get; private init; }

    /// <summary>
    /// The name of the currently visible page.
    /// </summary>
    public string? VisibleChildName
    {
        get => stack.VisibleChildName;
        set => stack.VisibleChildName = value;
    }

    /// <summary>
    /// Create a new <see cref="DynamicStackSidebar"/> instance.
    /// </summary>
    public DynamicStackSidebar()
    {
        OnPageSelected = new Event<string>();
        SetOrientation(Orientation.Horizontal);

        stack = new Stack();
		stack.MarginBottom = 6;
		stack.MarginTop = 6;
		stack.MarginStart = 6;
		stack.MarginEnd = 6;
        // stack.Hexpand = true; // Is this necessary?

        sidebar = new ListBox();
        sidebar.Vexpand = true;
        sidebar.OnRowActivated += OnSidebarRowActivated;
        sidebar.AddCssClass("navigation-sidebar");
        AddCssClass("sidebar");

        pages = new List<StackEntry>();

        StartChild = sidebar;
        EndChild = stack;

        ShrinkStartChild = false;
        StartChild.Hexpand = false;
        EndChild.Hexpand = true;
        Position = 0;
    }

    /// <summary>
    /// Populate the sidebar with the given pages.
    /// </summary>
    /// <param name="pages">The pages to be displayed.</param>
    public virtual void Populate(IEnumerable<(string, Widget)> pages)
    {
        foreach (StackEntry entry in this.pages)
        {
            stack.Remove(entry.Page);
            sidebar.Remove(entry.SidebarWidget);
            entry.SidebarWidget.Dispose();
            entry.Page.Dispose();
        }
        this.pages.Clear();

        foreach ((string name, Widget page) in pages)
        {
            Widget sidebarWidget = CreateWidget(name);
            AddEntry(name, page, sidebarWidget);
        }
    }

    /// <summary>
    /// Add an entry to the stack, and a corresponding row to the sidebar.
    /// </summary>
    /// <param name="name">Name of the entry.</param>
    /// <param name="page">The page widget.</param>
    /// <param name="sidebarWidget">The sidebar widget.</param>
    protected void AddEntry(string name, Widget page, Widget sidebarWidget)
    {
        stack.AddTitled(page, name, name);
        ListBoxRow row = new ListBoxRow();
        row.Child = sidebarWidget;
        row.Name = name;
        sidebar.Append(row);

        pages.Add(new StackEntry(name, page, row));
    }

    /// <summary>
    /// Called when a row in the sidebar is activated.
    /// </summary>
    /// <param name="sender">The sidebar.</param>
    /// <param name="args">The row-activated signal arguments.</param>
    protected virtual void OnSidebarRowActivated(ListBox sender, ListBox.RowActivatedSignalArgs args)
    {
        try
        {
			string? name = args.Row.Name;
            if (name == null)
                return;
            stack.VisibleChildName = name;
            OnPageSelected.Invoke(name);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Create a widget for the stack element with the specified title which
    /// will be displayed in the sidebar.
    /// </summary>
    /// <param name="title">Title of the stack element.</param>
    /// <returns>A widget to be displayed in the sidebar.</returns>
    protected virtual Widget CreateWidget(string title)
    {
        Label label = Label.New(title);
        label.Halign = Align.Start;
        return label;
    }
}
