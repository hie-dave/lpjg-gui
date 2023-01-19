using Gtk;
using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view displayed in the main window when no file is selected.
/// </summary>
public class NoFileView : Label, IView
{
	/// <summary>
	/// Create a new <see cref="NoFileView"/> instance.
	/// </summary>
	public NoFileView() : base()
	{
		SetMarkup("<i>No .ins file selected</i>");
		Xalign = 0.5f;
		Yalign = 0.5f;
		Hexpand = true;
		Vexpand = true;
	}

	/// <inheritdoc />
	public Widget GetWidget() => this;
}
