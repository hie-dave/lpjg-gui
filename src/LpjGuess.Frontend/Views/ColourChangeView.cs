using Gtk;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Utility;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// Wrapper for a <see cref="ColorDialogButton"/> which allows the user to
/// change a colour property.
/// </summary>
public class ColourChangeView : ViewBase<ColorDialogButton>
{
    /// <summary>
    /// The property name of the rgba property on the colour button.
    /// </summary>
    private const string rgbaProperty = "rgba";

    /// <summary>
    /// Called when the user changes the colour.
    /// </summary>
    public Event<Colour> OnChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="ColourChangeView"/> instance.
    /// </summary>
    public ColourChangeView() : base(new ColorDialogButton())
    {
        OnChanged = new Event<Colour>();
        widget.OnNotify += OnWidgetNotify;
    }

    /// <summary>
    /// Populate the widget with a colour.
    /// </summary>
    /// <param name="colour">The colour to populate the widget with.</param>
    public void Populate(Colour colour)
    {
        widget.Rgba = colour.ToRgba();
    }

    /// <summary>
    /// Called when the user changes the colour.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="args">The event arguments.</param>
    private void OnWidgetNotify(GObject.Object sender, GObject.Object.NotifySignalArgs args)
    {
        try
        {
            string property = args.Pspec.GetName();
            if (property != rgbaProperty)
                return;

            // Capture value now, as it may change before the event is handled.
            Colour colour = widget.Rgba.ToColour();
            OnChanged.Invoke(colour);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
