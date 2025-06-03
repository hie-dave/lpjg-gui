using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface to an editor widget.
/// </summary>
public interface IEditorView : IView
{
	/// <summary>
	/// Controls whether the text is editable.
	/// </summary>
	bool Editable { get; set; }

	/// <summary>
	/// Populate the view with the specified text.
	/// </summary>
	/// <param name="text">The text.</param>
	void Populate(string text);

	/// <summary>
	/// Append a line of text to the view.
	/// </summary>
	/// <param name="text">The text.</param>
	void AppendLine(string text);

	/// <summary>
	/// Clear the view.
	/// </summary>
	void Clear();

	/// <summary>
	/// Get the contents of the view.
	/// </summary>
	string GetContents();

	/// <summary>
	/// Event raised when the user has changed the contents of the view.
	/// </summary>
	Event OnChanged { get; }
}
