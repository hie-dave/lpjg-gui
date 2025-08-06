using Gtk;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view with a listbox widget with row selection causing a popover to appear,
/// displaying the corresponding child widget.
/// </summary>
public class ListBoxPopoverView : ListBoxNavigatorView
{
    /// <summary>
    /// The popover widget.
    /// </summary>
    private readonly Dictionary<Widget, Popover> popovers;

    /// <summary>
    /// Create a new <see cref="ListBoxPopoverView"/> instance.
    /// </summary>
    public ListBoxPopoverView() : base()
    {
        popovers = new Dictionary<Widget, Popover>();

        widget.Append(mainPage);
    }

    /// <inheritdoc/>
    public override void Populate(IEnumerable<INamedView> views)
    {
        RemovePopovers();
        base.Populate(views);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        RemovePopovers();
        base.Dispose();
    }

    /// <summary>
    /// Remove all popovers and free native resources.
    /// </summary>
    private void RemovePopovers()
    {
        foreach (Popover popover in popovers.Values)
            popover.Dispose();
        popovers.Clear();
    }

    /// <inheritdoc/>
    protected override void OnChildSelected(Widget widget)
    {
        Popover popover = popovers[widget];
        if (popover.Parent == null)
            popover.SetParent(GetRowWidget(widget));

        popover.Popup();
    }

    /// <inheritdoc/>
    protected override Widget AddChild(Widget widget)
    {
        Popover popover = new Popover();
        popover.SetChild(widget);
        popover.CascadePopdown = true;
        popovers.Add(widget, popover);
        return widget;
    }
}
