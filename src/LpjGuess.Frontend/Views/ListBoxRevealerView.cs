using Gtk;
using LpjGuess.Frontend.Utility.Gtk;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view with a listbox widget allowing navigation between a list of other
/// widgets displayed in a collapsible, scrollable editor pane.
/// </summary>
public class ListBoxRevealerView : ListBoxNavigatorView
{
    /// <summary>
    /// The scrolled window containing the selected editor.
    /// </summary>
    private readonly ScrolledWindow rhs;

    /// <summary>
    /// The child widgets.
    /// </summary>
    private readonly List<(Box Container, Widget Content, Button HideButton)> children;

    /// <summary>
    /// Create a new <see cref="ListBoxRevealerView"/> instance.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ListBoxRevealerView(ILogger<ListBoxNavigatorView> logger) : base(logger)
    {
        children = [];

        rhs = new ScrolledWindow();
        rhs.PropagateNaturalHeight = false;
        rhs.PropagateNaturalWidth = false;
        rhs.Hexpand = true;
        rhs.Vexpand = true;
        rhs.MinContentWidth = 360;
        rhs.Name = "rhs";
        rhs.Visible = false;

        widget.SetOrientation(Orientation.Horizontal);
        widget.Spacing = 6;
        widget.Vexpand = true;
        widget.Append(mainPage);
        widget.Append(rhs);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        base.Dispose();
    }

    /// <inheritdoc />
    protected override Widget AddChild(Widget widget)
    {
        Button hideButton = Button.NewFromIconName(Icons.GoPrevious);
        hideButton.Halign = Align.Start;
        hideButton.OnClicked += OnHideRevealer;

        Box box = Box.New(Orientation.Vertical, 6);
        box.Append(hideButton);
        box.Append(widget);
        children.Add((box, widget, hideButton));
        return box;
    }

    /// <inheritdoc />
    protected override void ClearChildWidgets()
    {
        rhs.Visible = false;
        rhs.Child = null;

        foreach ((Box container, Widget content, Button hideButton) in children)
        {
            hideButton.OnClicked -= OnHideRevealer;
            container.Remove(content);
            container.Dispose();
        }
        children.Clear();
    }

    /// <inheritdoc />
    protected override void OnChildSelected(Widget widget)
    {
        rhs.Child = widget;
        rhs.Visible = true;
    }

    private void OnHideRevealer(Button sender, EventArgs args)
    {
        rhs.Visible = false;
    }
}
