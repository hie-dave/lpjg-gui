using System.Runtime.InteropServices;
using Gio;
using Gtk;
using GtkSource;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
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
	/// The domain for actions added to the editor menu.
	/// </summary>
	private const string domain = "editor";

	/// <summary>
	/// Fallback style in dark mode.
	/// </summary>
	private const string defaultDarkStyle = "classic-dark";

	/// <summary>
	/// Default fallback style.
	/// </summary>
	private const string defaultLightStyle = "classic";

	/// <summary>
	/// The internal text view object.
	/// </summary>
	private readonly SourceView sourceView;

	/// <summary>
	/// The internal text buffer object.
	/// </summary>
	private readonly SourceBuffer buffer;

	/// <summary>
	/// Menu containing custom context options.
	/// </summary>
	private readonly Menu menu;

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

		SimpleActionGroup group = new SimpleActionGroup();
		menu = Menu.New();

		menu.AddMenuItem(group, "Change Style", OnChangeStyle, domain, "<Ctrl>L");

		sourceView.InsertActionGroup(domain, group);
		sourceView.ExtraMenu = menu;
	}

	private void OnChangeStyle()
	{
		try
		{
			StyleSchemeChooserWidget styleChooser = new StyleSchemeChooserWidget();
			StyleScheme? scheme = buffer.StyleScheme;
			if (scheme != null)
				styleChooser.StyleScheme = scheme;
			styleChooser.OnNotify += OnStyleChooserNotify;

			Window window = new Window();
			window.Title = "Choose Style";
			window.Child = styleChooser;
			window.TransientFor = MainView.Instance;
			window.Modal = true;
			window.OnCloseRequest += (_, __) =>
			{
				window.Dispose();
				return false;
			};
			window.Show();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	private void OnStyleChooserNotify(GObject.Object sender, GObject.Object.NotifySignalArgs args)
	{
		try
		{
			string property = args.Pspec.GetName();
			const string styleSchemeProperty = "style-scheme";
			if (args.Pspec.GetName() != styleSchemeProperty)
				return;

			StyleSchemeChooser? chooser = sender as StyleSchemeChooser;
			if (chooser == null)
				return;

			StyleScheme? scheme = chooser.StyleScheme;
			if (scheme == null)
				return;

			Configuration.Instance.EditorStyleName = scheme.Id;
			Configuration.Instance.Save();
			buffer.StyleScheme = scheme;
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
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
		{
			Console.WriteLine($"Failed to load default style '{name}'. Available schemes: {string.Join(", ", StyleSchemeManager.GetDefault().GetSchemeIds() ?? [])}");
			style = GetDefaultStyle();
		}

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
		// TODO: scroll to end.
		// Depends on support for out parameters in gir.core
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
