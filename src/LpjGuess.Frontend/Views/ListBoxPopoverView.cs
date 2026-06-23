using Gtk;
using LpjGuess.Frontend.Interfaces.Views;
using Microsoft.Extensions.Logging;

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
    /// <param name="logger">The logger.</param>
    public ListBoxPopoverView(ILogger<ListBoxNavigatorView> logger) : base(logger)
    {
        popovers = new Dictionary<Widget, Popover>();

        widget.Append(mainPage);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Remove all popovers and free native resources.
    /// </summary>
    private void RemovePopovers()
    {
        foreach (Popover popover in popovers.Values)
        {
            popover.Child = null;
            if (popover.Parent != null)
                popover.Unparent();
            popover.Dispose();
        }
        popovers.Clear();
    }

    /// <inheritdoc />
    protected override void ClearChildWidgets() => RemovePopovers();

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
