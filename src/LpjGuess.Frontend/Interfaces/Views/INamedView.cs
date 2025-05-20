namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Represents a named view component that can be displayed in a grid layout.
/// </summary>
public interface INamedView
{
    /// <summary>
    /// The label text to display for this view component.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The view that provides the control interface.
    /// </summary>
    IView View { get; }
}
