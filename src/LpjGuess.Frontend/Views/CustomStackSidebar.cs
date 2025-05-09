using System.Diagnostics;
using Gtk;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A StackSidebar which allows the caller to use custom widgets in the sidebar
/// switcher for navigation (stock StackSidebar allows only labels).
/// </summary>
public class CustomStackSidebar<T> : Paned
{
    /// <summary>
    /// The stack which the sidebar manages.
    /// </summary>
    private Stack stack;

    /// <summary>
    /// The sidebar which allows navigation between the widgets of the stack.
    /// </summary>
    private readonly ListBox sidebar;

    /// <summary>
    /// Dictionary mapping page IDs (page names) to pages in the stack.
    /// This is a workaround to allow us to disambiguate entries when we have
    /// multiple entries with the same name.
    /// </summary>
    private readonly Dictionary<string, StackEntry> pages;

    /// <summary>
    /// Function which renders a data value to a widget which will be displayed
    /// in the sidebar.
    /// </summary>
    private readonly Func<T, Widget> renderer;

    /// <summary>
    /// Called when the user selects a page.
    /// </summary>
    public Event<T> OnPageSelected { get; private init; }

    /// <summary>
    /// The name of the currently visible page.
    /// </summary>
    public string? VisibleChildName
    {
        get => stack.VisibleChildName;
        set => stack.VisibleChildName = value;
    }

    /// <summary>
    /// Create a new <see cref="CustomStackSidebar{T}"/> instance.
    /// </summary>
    public CustomStackSidebar(Func<T, Widget> renderer)
    {
        this.renderer = renderer;
        OnPageSelected = new Event<T>();
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

        pages = new Dictionary<string, StackEntry>();

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
    public virtual void Populate(IEnumerable<(T, Widget)> pages)
    {
        foreach (StackEntry entry in this.pages.Values)
        {
            stack.Remove(entry.Page);
            sidebar.Remove(entry.SidebarWidget);
            entry.SidebarWidget.Dispose();
            entry.Page.Dispose();
        }
        this.pages.Clear();

        foreach ((T data, Widget page) in pages)
        {
            string id = Guid.NewGuid().ToString();
            Widget sidebarWidget = CreateWidget(data);
            AddEntry(id, data, page, sidebarWidget);
        }
    }

    /// <summary>
    /// Add an entry to the stack, and a corresponding row to the sidebar.
    /// </summary>
    /// <param name="id">ID of the entry.</param>
    /// <param name="data">The data associated with the entry.</param>
    /// <param name="page">The page widget.</param>
    /// <param name="sidebarWidget">The sidebar widget.</param>
    protected void AddEntry(string id, T data, Widget page, Widget sidebarWidget)
    {
        // We can safely use the ID for the title as well, because the title is
        // not (necessarily) rendered anywhere in the UI.
        stack.AddTitled(page, id, id);
        ListBoxRow row = new ListBoxRow();
        row.Child = sidebarWidget;
        row.Name = id;
        sidebar.Append(row);

        pages[id] = new StackEntry(data, page, row);
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
			string? id = args.Row.Name;
            if (id == null)
                return;
            stack.VisibleChildName = id;
            if (!pages.ContainsKey(id))
                Debug.WriteLine($"{GetType().Name}: Invalid page ID: {id}. Likely a use-after-free bug");
            StackEntry entry = pages[id];
            OnPageSelected.Invoke(entry.Data);
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
    /// <param name="data">The data value to be rendered to the sidebar.</param>
    /// <returns>A widget to be displayed in the sidebar.</returns>
    protected virtual Widget CreateWidget(T data)
    {
        return renderer(data);
    }

    private class StackEntry
    {
        public T Data { get; private init; }
        public Widget Page { get; private init; }
        public Widget SidebarWidget { get; private init; }

        public StackEntry(T data, Widget page, Widget sidebarWidget)
        {
            Data = data;
            Page = page;
            SidebarWidget = sidebarWidget;
        }
    }
}
