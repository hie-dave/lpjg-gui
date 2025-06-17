using Gtk;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A wrapper around a Gtk checkbutton.
/// </summary>
public class CheckBoxView : ViewBase<CheckButton>
{
    /// <summary>
    /// Called when the check button is toggled by the user.
    /// </summary>
    public Event<bool> OnToggled { get; private init; }

    /// <summary>
    /// The active state of the check button.
    /// </summary>
    public bool Active => widget.Active;

    /// <summary>
    /// Create a new <see cref="CheckBoxView"/> instance.
    /// </summary>
    public CheckBoxView() : base(new CheckButton())
    {
        OnToggled = new Event<bool>();
        widget.OnToggled += OnCheckButtonToggled;
    }

    /// <summary>
    /// Called when the check button is toggled by the user.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnCheckButtonToggled(CheckButton sender, EventArgs args)
    {
        try
        {
            OnToggled.Invoke(widget.Active);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
