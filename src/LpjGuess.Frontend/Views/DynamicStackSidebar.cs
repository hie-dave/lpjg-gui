using System.Diagnostics;
using ExtendedXmlSerializer.ExtensionModel.AttachedProperties;
using Gtk;
using LpjGuess.Frontend.Utility.Gtk;

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
    /// Create a new <see cref="DynamicStackSidebar"/> instance.
    /// </summary>
    public DynamicStackSidebar()
    {
        SetOrientation(Orientation.Horizontal);

        stack = new Stack();
        sidebar = new ListBox();
        sidebar.OnRowActivated += OnSidebarRowActivated;

        pages = new List<StackEntry>();

        StartChild = sidebar;
        EndChild = stack;

        // TODO: ensure StartChild (sidebar) can't be shrunk.
    }

    /// <summary>
    /// Populate the sidebar with the given pages.
    /// </summary>
    /// <param name="pages">The pages to be displayed.</param>
    public void Populate(IEnumerable<(string, Widget)> pages)
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
            StackEntry entry = new StackEntry(name, page, CreateWidget(name));
            stack.AddTitled(page, name, name);

            Widget sidebarWidget = CreateWidget(name);
            ListBoxRow row = new ListBoxRow();
            row.Child = sidebarWidget;
            row.Name = name;
            sidebar.Append(row);

            this.pages.Add(new StackEntry(name, page, row));
        }
    }

    private void OnSidebarRowActivated(ListBox sender, ListBox.RowActivatedSignalArgs args)
    {
        try
        {
			string? name = args.Row.Name;
            if (name == null)
                return;
            stack.VisibleChildName = name;
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
    private Widget CreateWidget(string title)
    {
        return Label.New(title);
    }
}
