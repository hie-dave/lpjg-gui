using System.Text;
using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays text in an optionally editable view.
/// </summary>
public class EditorView : ViewBase<ScrolledWindow>, IEditorView
{
	/// <summary>
	/// The contents of the view.
	/// </summary>
	private readonly StringBuilder contents;

	/// <summary>
	/// The internal text view object.
	/// </summary>
	private readonly TextView textView;

	/// <inheritdoc />
	public bool Editable
	{
		get => textView.Editable;
		set => textView.Editable = value;
	}

	/// <inheritdoc />
	public Event OnChanged { get; private init; }

	/// <summary>
	/// Create a new <see cref="EditorView"/> instance which displays the
	/// specified text.
	/// </summary>
	public EditorView() : base(new ScrolledWindow())
	{
		OnChanged = new Event();

		contents = new StringBuilder();
		TextBuffer buffer = TextBuffer.New(null);
		textView = TextView.NewWithBuffer(buffer);
		textView.Vexpand = true;
		textView.Monospace = true;

		widget.Child = textView;
		ConnectEvents();
	}

    /// <inheritdoc />
    public void AppendLine(string line)
	{
		DisconnectEvents();

		contents.AppendLine(line);
		string text = contents.ToString();
		TextBuffer buffer = textView.GetBuffer();
		buffer.SetText(text, Encoding.UTF8.GetByteCount(text));

		ConnectEvents();
	}

	/// <inheritdoc />
	public string? GetContents()
	{
		return textView.GetBuffer().Text;
	}

	/// <inheritdoc />
	public void Clear()
	{
		textView.GetBuffer().SetText(string.Empty, 0);
		contents.Clear();
	}

	/// <inheritdoc />
	public override void Dispose()
	{
		DisconnectEvents();
		OnChanged.DisconnectAll();
		base.Dispose();
	}

	/// <summary>
	/// Connect events.
	/// </summary>
	private void ConnectEvents()
	{
		textView.GetBuffer().OnChanged += OnTextBufferChanged;
	}

	/// <summary>
	/// Disconnect events.
	/// </summary>
	private void DisconnectEvents()
	{
		textView.GetBuffer().OnChanged -= OnTextBufferChanged;
	}

	/// <summary>
	/// Called when the text buffer changes.
	/// </summary>
	private void OnTextBufferChanged(TextBuffer sender, EventArgs args)
	{
		try
		{
			OnChanged.Invoke();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}
}
