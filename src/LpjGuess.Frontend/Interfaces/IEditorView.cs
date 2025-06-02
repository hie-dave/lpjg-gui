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
	/// Append a line of text to the view's contents.
	/// </summary>
	/// <param name="line">The text.</param>
	void AppendLine(string line);

	/// <summary>
	/// Empty all text from the output view.
	/// </summary>
	void Clear();

	/// <summary>
	/// Get the contents of the view.
	/// </summary>
	string? GetContents();

	/// <summary>
	/// Event raised when the user has changed the contents of the view.
	/// </summary>
	Event OnChanged { get; }
}
