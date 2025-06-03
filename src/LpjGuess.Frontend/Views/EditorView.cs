using Gtk;
using GtkSource;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;

using SourceBuffer = GtkSource.Buffer;
using SourceView = GtkSource.View;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays text in an optionally editable view.
/// </summary>
/// <remarks>
/// This needs to be refactored by extracting a separate class for the log view.
/// That class needs clear/appendline functionality, and shouldn't be editable,
/// which doesn't really make sense for an "editor" view.
/// </remarks>
public class EditorView : ViewBase<ScrolledWindow>, IEditorView
{
	/// <summary>
	/// Fallback style in dark mode.
	/// </summary>
	private const string defaultDarkStyle = "oblivion";

	/// <summary>
	/// Default fallback style.
	/// </summary>
	private const string defaultLightStyle = "Classic";

	/// <summary>
	/// The internal text view object.
	/// </summary>
	private readonly SourceView sourceView;

	/// <summary>
	/// The internal text buffer object.
	/// </summary>
	private readonly SourceBuffer buffer;

	/// <inheritdoc />
	public bool Editable
	{
		get => sourceView.Editable;
		set => sourceView.Editable = value;
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

		buffer = new SourceBuffer();
		sourceView = SourceView.NewWithBuffer(buffer);
		sourceView.Vexpand = true;
		sourceView.Monospace = true;
		sourceView.ShowLineNumbers = true;

		// Attempt to load a style scheme from the user settings.
		StyleScheme? style = GetStyleScheme();
		if (style != null)
			buffer.StyleScheme = style;

		widget.Child = sourceView;
		ConnectEvents();
	}

    private StyleScheme? GetStyleScheme()
    {
		StyleScheme? style = null;

		// Attempt to load a style scheme from the user settings.
		string? name = Configuration.Instance.EditorStyleName;
		if (name != null)
			style = StyleSchemeManager.GetDefault().GetScheme(name);

		// Fallback to a default style scheme if the user's choice can't be
		// loaded (or the user hasn't set a style scheme).
		if (style == null)
			style = GetDefaultStyle();

		return style;
    }

    /// <summary>
    /// Get the a fallback style scheme to be used when a default is either not
    /// set or can't be loaded.
    /// </summary>
    private static StyleScheme? GetDefaultStyle()
	{
		string name = GetDefaultStyleName();
		return StyleSchemeManager.GetDefault().GetScheme(name);
	}

	/// <summary>
	/// Get the default style name.
	/// </summary>
	private static string GetDefaultStyleName()
	{
#if LINUX
		try
		{
			var styleManager = Adw.StyleManager.GetDefault();
			if (styleManager != null && styleManager.Dark)
				return defaultDarkStyle;
		}
		catch
		{
			// Ignore errors here and use fallback styles.
		}
#endif
		return Configuration.Instance.PreferDarkMode ? defaultDarkStyle : defaultLightStyle;
	}

	/// <inheritdoc />
	public void Populate(string text)
	{
		DisconnectEvents();

		buffer.Text = text;

		ConnectEvents();
	}

	/// <inheritdoc />
	public void AppendLine(string text)
	{
		buffer.Text += text + Environment.NewLine;
	}

	/// <inheritdoc />
	public void Clear()
	{
		buffer.Text = string.Empty;
	}

	/// <inheritdoc />
	public string GetContents()
	{
		return buffer.Text ?? string.Empty;
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
		buffer.OnChanged += OnTextBufferChanged;
	}

	/// <summary>
	/// Disconnect events.
	/// </summary>
	private void DisconnectEvents()
	{
		buffer.OnChanged -= OnTextBufferChanged;
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
