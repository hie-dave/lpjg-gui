using Gtk;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for a dummy factor.
/// </summary>
public class DummyFactorView : ViewBase<Label>, IDummyFactorView
{
    /// <summary>
    /// Create a new <see cref="DummyFactorView"/> instance.
    /// </summary>
    public DummyFactorView() : base(Label.New("A factor which makes no changes"))
    {
        widget.Halign = Align.Start;
    }
}
