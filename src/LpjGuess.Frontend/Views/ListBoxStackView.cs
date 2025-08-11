using Gtk;
using GtkSource;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;
using Microsoft.Extensions.Logging;

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
public class ListBoxStackView : ListBoxNavigatorView
{
    /// <summary>
    /// The stack widget.
    /// </summary>
    private readonly Stack stack;

    /// <summary>
    /// Create a new <see cref="ListBoxStackView"/> instance.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ListBoxStackView(ILogger<ListBoxNavigatorView> logger) : base(logger)
    {
        stack = new Stack();
        stack.Vexpand = true;

        // ListBox goes into the stack, stack goes into the main widget.
        stack.AddChild(mainPage);
        stack.TransitionType = StackTransitionType.SlideLeftRight;
        stack.VisibleChild = mainPage;

        widget.SetOrientation(Orientation.Vertical);
        widget.Append(stack);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        base.Dispose();
    }

    /// <inheritdoc />
    protected override void OnChildSelected(Widget widget)
    {
        stack.VisibleChild = widget;
    }

    /// <inheritdoc />
    protected override Widget AddChild(Widget widget)
    {
        Button backButton = Button.NewFromIconName(Icons.GoPrevious);
        backButton.Halign = Align.Start;
        backButton.OnClicked += OnShowListBox;
        Box container = Box.New(Orientation.Vertical, 6);
        container.Append(backButton);
        container.Append(widget);

        stack.AddChild(container);
        return container;
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
