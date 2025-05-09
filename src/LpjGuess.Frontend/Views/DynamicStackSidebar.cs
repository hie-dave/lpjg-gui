using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A <see cref="CustomStackSidebar{T}"/> which allows the user to add and
/// remove tabs from the stack.
/// </summary>
public class DynamicStackSidebar<T> : CustomStackSidebar<T>
{
    /// <summary>
    /// Spacing, in pixels, between elements in the title box in the sidebar.
    /// </summary>
    private const int spacing = 6;

    /// <summary>
    /// Name of the widget associated with the 'add' menu option.
    /// </summary>
    private const string addName = "add-element";

    /// <summary>
    /// The name of the previously selected page.
    /// </summary>
    private string? previouslySelected = null;

    /// <summary>
    /// Event which is raised when the user wants to add an item.
    /// </summary>
    public Event OnAdd { get; private init; }

    /// <summary>
    /// Event which is raised when the user wants to remove an item.
    /// </summary>
    public Event<T> OnRemove { get; private init; }

    /// <summary>
    /// The text displayed on "Add" buttons.
    /// </summary>
    public string AddText { get; set; }

    /// <summary>
    /// Create a new <see cref="DynamicStackSidebar{T}"/> instance.
    /// </summary>
    public DynamicStackSidebar(Func<T, Widget> renderer) : base(renderer)
    {
        OnAdd = new Event();
        OnRemove = new Event<T>();
        AddText = "Add";
    }

    /// <summary>
    /// Populate the view with the given pages, as well as an "Add File" entry.
    /// </summary>
    /// <param name="pages">The pages to be displayed.</param>
    public override void Populate(IEnumerable<(T, Widget)> pages)
    {
        base.Populate(pages);

        // Add an "Add Element" entry.
        Label label = Label.New(AddText);
        label.Halign = Align.Start;
        label.Hexpand = true;
        AddEntry(addName, default!, new Box(), label);
    }

    /// <summary>
    /// Create a widget for the stack element with the specified title which
    /// will be displayed in the sidebar. This implementation creates a label
    /// with a menu button with a delete action.
    /// </summary>
    /// <param name="data">The element for which a sidebar widget is needed.</param>
    /// <returns>A widget to be displayed in the sidebar.</returns>
    protected override Widget CreateWidget(T data)
    {
        Widget coreWidget = base.CreateWidget(data);

        Button menuButton = new Button();
        menuButton.IconName = Icons.Delete;
        menuButton.AddCssClass("flat");
        menuButton.Halign = Align.End;
        menuButton.OnClicked += (_, __) => RemoveFile(data);

        Box box = Box.New(Orientation.Horizontal, spacing);
        box.Append(coreWidget);
        box.Append(menuButton);

        return box;
    }

    /// <summary>
    /// Callback for the sidebar's "row-activated" signal. Handle the "Add File"
    /// option, if that's what was clicked. Otherwise, the event will propagate
    /// up to the base class.
    /// </summary>
    /// <param name="sender">The sidebar.</param>
    /// <param name="args">The row-activated signal arguments.</param>
    protected override void OnSidebarRowActivated(ListBox sender, ListBox.RowActivatedSignalArgs args)
    {
        try
        {
			string? name = args.Row.Name;
            if (name == null)
                return;

            if (name == addName)
            {
                if (previouslySelected != null)
                    VisibleChildName = previouslySelected;

                OnAdd.Invoke();
                return;
            }
            previouslySelected = VisibleChildName;
            base.OnSidebarRowActivated(sender, args);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user wants to remove an instruction file.
    /// </summary>
    /// <param name="data">The element to be removed.</param>
    private void RemoveFile(T data)
    {
        try
        {
            OnRemove.Invoke(data);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
