using Adw;
using Gtk;
using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// An ActionRow which contains a button and text.
/// </summary>
public class ButtonRow : ActionRow, IPropertyView
{
	/// <summary>
	/// Create a new <see cref="ButtonRow"/> instance.
	/// </summary>
	/// <param name="text">Text to be displayed on the button.</param>
	/// <param name="callback">Action to be invoked when the button is clicked.</param>
	public ButtonRow(string text, Action callback)
	{
		Title = text;
		Activatable = true;
		OnActivated += (_, __) => callback();
	}

	/// <inheritdoc />
	public string GetDescription() => throw new NotImplementedException();

	/// <inheritdoc />
	public Widget GetWidget() => this;

	/// <inheritdoc />
	public string PropertyName() => throw new NotImplementedException();
}
