using System.Text;
using Adw;
using Gio;
using Gtk;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Utility;
using LpjGuess.Frontend.Utility.Gtk;

// Application is ambiguous between Adw, Gio, and Gtk.
using Action = System.Action;
using Application = Adw.Application;
using ApplicationWindow = Adw.ApplicationWindow;
using MessageDialog = Adw.MessageDialog;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// The main window.
/// </summary>
public class MainView : ApplicationWindow, IMainView
{
	/// <summary>
	/// Margin (in px) around window contents and winow border.
	/// </summary>
	private const int margin = 8;

	/// <summary>
	/// Spacing (in px) between internal elements of the window.
	/// </summary>
	private const int spacing = 5;

	/// <summary>
	/// The application instance.
	/// </summary>
	/// <remarks>
	/// Technically this will be null between the invocation of the static
	/// constructor, and the invocation of the instance constructor. In
	/// practice, this property is never accessed during that (brief) window.
	/// </remarks>
#pragma warning disable CS8618
	public static Application AppInstance { get; private set; }

	/// <summary>
	/// The main window instance.
	/// </summary>
	public static MainView Instance { get; private set; }
#pragma warning restore CS8618

	/// <summary>
	/// The application menu widget. Actions added via
	/// <see cref="AddMenuItem(string, Action, string?)"/> will be added here.
	/// </summary>
	private readonly Menu menu;

	/// <summary>
	/// The main window contents.
	/// </summary>
	private readonly Box main;

	/// <summary>
	/// Label containing the window title.
	/// </summary>
	private readonly Label title;

	/// <summary>
	/// Label containing the window subtitle. If no subtitle is set, this will
	/// not be visible.
	/// </summary>
	private readonly Label subtitle;

	/// <summary>
	/// The main child of the window.
	/// </summary>
	private IView? childView;

	/// <summary>
	/// Called when the user wants to open a file.
	/// </summary>
	public event Action<string>? OpenFile;

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="app"></param>
	public MainView(Application app) : base()
	{
		AppInstance = app;
		Instance = this;

		SetApplication(app);

		CssProvider provider = CssProvider.New();
		provider.LoadFromEmbeddedResource("LpjGuess.Frontend.css.style.css");
		uint priority = (uint)Gtk.Constants.STYLE_PROVIDER_PRIORITY_APPLICATION;
		if (Display != null)
			StyleContext.AddProviderForDisplay(Display, provider, priority);

		menu = Menu.New();

		MenuButton menuButton = new MenuButton();
		menuButton.MenuModel = menu;
		menuButton.IconName = "open-menu-symbolic";

		Button openInsFile = Button.NewWithLabel("Open");
		openInsFile.OnClicked += OnOpenInsFile;

		title = Label.New(Title);
		title.AddCssClass(StyleClasses.Title);

		// The file path subtitle is only shown when a file is open.
		subtitle = new Label();
		subtitle.AddCssClass(StyleClasses.Subtitle);
		subtitle.Ellipsize = Pango.EllipsizeMode.Middle;
		subtitle.Hide();

		Box titleBox = new Box();
		titleBox.SetOrientation(Orientation.Vertical);
		titleBox.Valign = Align.Center;
		titleBox.Append(title);
		titleBox.Append(subtitle);

		var header = new Adw.HeaderBar();
		header.CenteringPolicy = CenteringPolicy.Strict;
		header.TitleWidget = titleBox;
		header.PackStart(openInsFile);
		header.PackEnd(menuButton);

		main = new Box();
		main.SetOrientation(Orientation.Vertical);
		main.MarginTop = margin;
		main.MarginBottom = margin;
		main.MarginStart = margin;
		main.MarginEnd = margin;

		Box contents = new Box();
		contents.SetOrientation(Orientation.Vertical);
		contents.Append(header);
		contents.Append(main);
		SetContent(contents);

		Maximized = true;
	}

	/// <inheritdoc />
	public void SetChild(IView view)
	{
		// Remove the old fileView.
		if (childView != null)
			main.Remove(childView.GetWidget());

		childView = view;
		main.Append(view.GetWidget());
	}

	/// <inheritdoc />
	public void SetTitle(string title, string? subtitle = null)
	{
		this.title.SetText(title);
		if (subtitle == null)
			this.subtitle.Hide();
		else
		{
			this.subtitle.Show();
			this.subtitle.SetText(subtitle);
		}
	}

	/// <inheritdoc />
	public Widget GetWidget()
	{
		return this;
	}

	/// <inheritdoc />
	public void AddMenuItem(string name, Action callback, string? hotkey = null)
	{
		const string domain = "app";
		menu.AddMenuItem(domain, name, callback, hotkey);
	}

	/// <summary>
	/// Perform the specified action on the main thread.
	/// </summary>
	/// <param name="callback">Action to be performed.</param>
	public static void RunOnMainThread(Action callback)
	{
		GLib.Functions.IdleAddFull(0, _ =>
		{
			callback();
			return false;
		});
	}

	/// <summary>
	/// Error handler routine.
	/// </summary>
	/// <param name="error"></param>
	public void ReportError(Exception error)
	{
		Console.Error.WriteLine(error.ToString());

		TextBuffer buffer = TextBuffer.New(null);
		buffer.SetText(error.ToString(), Encoding.UTF8.GetByteCount(error.ToString()));
		TextView text = new TextView();
		text.SetBuffer(buffer);
		text.Editable = false;
		text.Vexpand = true;
		text.Hexpand = true;
		text.Monospace = true;

		ScrolledWindow scroller = new ScrolledWindow();
		scroller.Child = text;
		scroller.PropagateNaturalHeight = true;
		scroller.PropagateNaturalWidth = true;
		// fixme - need to make the error dialogs wide enough to show full call
		// stack without scrolling.
		// scroller.HscrollbarPolicy = PolicyType.Never;
		scroller.Vexpand = true;
		scroller.Hexpand = true;

		Expander expander = new Expander();
		expander.Child = scroller;
		expander.Label = "Details";

		// Show error in a dialog box for now.
		var dialog = new MessageDialog();
		dialog.Modal = true;
		dialog.TransientFor = this;
		dialog.Title = "Error";
		dialog.Heading = "Error";
		dialog.Body = error.Message;
		dialog.Resizable = true;
		dialog.ExtraChild = expander;
		dialog.DefaultWidth = 480;
		dialog.AddResponse("ok", "_Ok");
		dialog.OnResponse += OnCloseErrorDialog;
		dialog.Present();

		// Console.Error.WriteLine(error);
	}

	private void OnCloseErrorDialog(MessageDialog sender, MessageDialog.ResponseSignalArgs args)
	{
		try
		{
			sender.OnResponse -= OnCloseErrorDialog;
			sender.Dispose();
		}
		catch (Exception error)
		{
			Console.WriteLine(error);
		}
	}

	/// <summary>
	/// Called when the user wants to open a file. Opens a file chooser dialog.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnOpenInsFile(Button sender, EventArgs args)
	{
		try
		{
			Rows.FileChooserRow.FixFileChooser();
			FileChooserNative fileChooser = FileChooserNative.New(
				"Open Instruction File",
				this,
				FileChooserAction.Open,
				"Open",
				"Cancel"
			);
			fileChooser.SetModal(true);
			FileFilter filterIns = FileFilter.New();
			filterIns.AddPattern("*.ins");
			filterIns.Name = "Instruction Files (*.ins)";
			fileChooser.AddFilter(filterIns);

			FileFilter filterAll = FileFilter.New();
			filterAll.AddPattern("*");
			filterAll.Name = "All Files";
			fileChooser.AddFilter(filterAll);

			fileChooser.OnResponse += OnInsFileSelected;

			fileChooser.Show();
		}
		catch (Exception error)
		{
			ReportError(error);
		}
	}

	/// <summary>
	/// Called when the file chooser dialog finishes running (ie after the user
	/// chooses a file).
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnInsFileSelected(NativeDialog sender, NativeDialog.ResponseSignalArgs args)
	{
		try
		{
			if (sender is FileChooserNative fileChooser &&
				args.ResponseId == (int)ResponseType.Accept)
			{
				string? selectedFile = fileChooser.GetFile()?.GetPath();
				if (!string.IsNullOrEmpty(selectedFile))
					OpenFile?.Invoke(selectedFile);
			}
			sender.Dispose();
		}
		catch (Exception error)
		{
			ReportError(error);
		}
	}
}
