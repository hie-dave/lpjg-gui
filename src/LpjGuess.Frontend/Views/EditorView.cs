using System.Text;
using Gtk;
using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays text in an optionally editable view.
/// </summary>
public class EditorView : IEditorView
{
	/// <summary>
	/// The scrolled window windget containing the textview.
	/// </summary>
	private readonly ScrolledWindow scroller;

	/// <summary>
	/// The internal text view object.
	/// </summary>
	private readonly TextView textView;

	/// <summary>
	/// The text shown in the view.
	/// </summary>
	private readonly StringBuilder contents;

	/// <inheritdoc />
	public bool Editable { get => textView.Editable; set => textView.Editable = value; }

	/// <summary>
	/// Create a new <see cref="EditorView"/> instance which displays the
	/// specified text.
	/// </summary>
	public EditorView()
	{
		this.contents = new StringBuilder();
		TextBuffer buffer = TextBuffer.New(null);
		textView = TextView.NewWithBuffer(buffer);
		textView.Vexpand = true;
		textView.Monospace = true;

		scroller = new ScrolledWindow();
		scroller.Child = textView;

		ConnectEvents();
	}

	/// <inheritdoc />
	public void AppendLine(string line)
	{
		contents.AppendLine(line);
		string text = contents.ToString();
		TextBuffer buffer = textView.GetBuffer();
		buffer.SetText(text, Encoding.UTF8.GetByteCount(text));
	}

	/// <inheritdoc />
	public void Clear()
	{
		textView.GetBuffer().SetText("", 0);
		contents.Clear();
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		DisconnectEvents();
		scroller.Dispose();
	}

	/// <inheritdoc />
	public Widget GetWidget() => scroller;

	/// <summary>
	/// Connect all events.
	/// </summary>
	private void ConnectEvents()
	{
	}

	/// <summary>
	/// Disconnect all events.
	/// </summary>
	private void DisconnectEvents()
	{
	}
}
