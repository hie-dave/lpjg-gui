using Adw;
using Gtk;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// An ActionRow which contains an 'add item' button.
/// </summary>
public class AddElementRow : ButtonRow
{
	/// <summary>
	/// Create a new <see cref="ButtonRow"/> instance.
	/// </summary>
	/// <param name="text">Text to be displayed on the button.</param>
	/// <param name="callback">Action to be invoked when the button is clicked.</param>
	public AddElementRow(string text, Action callback) : base(text, callback)
	{
		AddPrefix(Image.NewFromIconName(Icons.AddItem));
	}
}
