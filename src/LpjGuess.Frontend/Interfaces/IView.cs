using Gtk;

namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface for views.
/// </summary>
public interface IView : IDisposable
{
	/// <summary>
	/// Get the main widget owned by this view.
	/// </summary>
	internal Widget GetWidget();
}
