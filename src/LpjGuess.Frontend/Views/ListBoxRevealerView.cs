using Gtk;
using LpjGuess.Frontend.Utility.Gtk;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view with a listbox widget allowing navigation between a list of other
/// widgets displayed in a collapsible GtkRevealer.
/// </summary>
public class ListBoxRevealerView : ListBoxNavigatorView
{
    /// <summary>
    /// The revealer widget.
    /// </summary>
    private readonly Revealer revealer;

    /// <summary>
    /// The scrolled window containing the contents of the collapsible revealer.
    /// </summary>
    private readonly ScrolledWindow rhs;

    /// <summary>
    /// The child widgets.
    /// </summary>
    private readonly List<Widget> children;

    /// <summary>
    /// Create a new <see cref="ListBoxRevealerView"/> instance.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ListBoxRevealerView(ILogger<ListBoxNavigatorView> logger) : base(logger)
    {
        children = new List<Widget>();

        rhs = new ScrolledWindow();
        rhs.PropagateNaturalHeight = true;
        rhs.PropagateNaturalWidth = true;
        rhs.Name = "rhs";

        revealer = new Revealer();
        revealer.TransitionType = RevealerTransitionType.SlideRight;
        revealer.TransitionDuration = 250; // ms
        revealer.RevealChild = false;
        revealer.Hexpand = false;
        revealer.SetChild(rhs);

        widget.SetOrientation(Orientation.Horizontal);
        widget.Spacing = 6;
        widget.Append(mainPage);
        widget.Append(revealer);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        rhs.Child = null;
        foreach (Widget widget in children)
            widget.Dispose();
        children.Clear();
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
        children.Add(box);
        return box;
    }

    /// <inheritdoc />
    protected override void OnChildSelected(Widget widget)
    {
        revealer.Child = widget;
        if (!revealer.RevealChild)
            revealer.RevealChild = true;
    }

    private void OnHideRevealer(Button sender, EventArgs args)
    {
        revealer.RevealChild = false;
    }
}
