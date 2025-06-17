using System.Text;
using Adw;
using Gio;
using Gtk;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Utility;
using LpjGuess.Frontend.Utility.Gtk;

// Application is ambiguous between Adw, Gio, and Gtk.
using Action = System.Action;
using Application = Adw.Application;
using ApplicationWindow = Adw.ApplicationWindow;
using MessageDialog = Adw.MessageDialog;
using FileChooserDialog = LpjGuess.Frontend.Views.Dialogs.FileChooserDialog;
using StyleContext = Gtk.StyleContext;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// The main window.
/// </summary>
public class MainView : ApplicationWindow, IMainView
{
	/// <summary>
	/// Domain for app-level menu items.
	/// </summary>
	private const string appDomain = "app";

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

	/// <inheritdoc />
	public Event<string> OnOpen { get; private init; }

	/// <inheritdoc />
	public Event<string> OnNewFromInstructionFile { get; private init; }

	/// <inheritdoc />
	public Event<string> OnNew { get; private init; }

	/// <inheritdoc /> 
	public Event OnClose { get; private init; }

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="app"></param>
	public MainView(Application app) : base()
	{
		AppInstance = app;
		Instance = this;

		OnOpen = new Event<string>();
		OnNewFromInstructionFile = new Event<string>();
		OnNew = new Event<string>();
		OnClose = new Event();

		OnCloseRequest += OnClosed;

		SetApplication(app);

		menu = Menu.New();

		MenuButton menuButton = new MenuButton();
		menuButton.MenuModel = menu;
		menuButton.IconName = "open-menu-symbolic";

		Menu openMenu = Menu.New();
		openMenu.AddApplicationMenuItem(appDomain, "Open", OnOpenWorkspace, "<Ctrl>O");
		openMenu.AddApplicationMenuItem(appDomain, "New", OnNewWorkspace, "<Ctrl>N");
		openMenu.AddApplicationMenuItem(appDomain, "New from Instruction File", OnOpenInsFile, "<Ctrl><Shift>N");

		MenuButton openInsFile = new MenuButton();
		openInsFile.Label = "Open";
		openInsFile.AlwaysShowArrow = true;
		openInsFile.Direction = ArrowType.Down;
		openInsFile.MenuModel = openMenu;

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

	/// <summary>
	/// Initialise the view.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	public async System.Threading.Tasks.Task InitialiseAsync(CancellationToken ct = default)
	{
		CssProvider provider = CssProvider.New();
		await provider.LoadFromEmbeddedResourceAsync("LpjGuess.Frontend.css.style.css", ct);
		uint priority = Gtk.Constants.STYLE_PROVIDER_PRIORITY_APPLICATION;
		if (Display != null)
			StyleContext.AddProviderForDisplay(Display, provider, priority);
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
		Title = title;
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
		menu.AddApplicationMenuItem(appDomain, name, callback, hotkey);
	}

	/// <summary>
	/// Perform the specified action on the main thread.
	/// </summary>
	/// <param name="callback">Action to be performed.</param>
	public static void RunOnMainThread(Action callback)
	{
		GLib.Functions.IdleAdd(0, new GLib.SourceFunc(() =>
		{
			try
			{
				callback();
			}
			catch (Exception error)
			{
				Instance.ReportError(error);
			}
			return false;
		}));
	}

	/// <summary>
	/// Error handler routine.
	/// </summary>
	/// <param name="error"></param>
	public void ReportError(Exception error)
	{
		if (error is TaskCanceledException || error is OperationCanceledException)
			return;

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

	private void OnNewWorkspace()
	{
		try
		{
			var fileChooser = FileChooserDialog.Save(
				"Save Workspace",
				"Workspaces",
				$"*{Workspace.DefaultFileExtension}",
				true);
			fileChooser.OnFileSelected.ConnectTo(OnNew);
			fileChooser.Run();
		}
		catch (Exception error)
		{
			ReportError(error);
		}
	}

	private void OnOpenWorkspace()
	{
		try
		{
			var fileChooser = FileChooserDialog.Open(
				"Open Workspace",
				"Workspaces",
				$"*{Workspace.DefaultFileExtension}",
				true,
				false);
			fileChooser.OnFileSelected.ConnectTo(OnOpen);
			fileChooser.Run();
		}
		catch (Exception error)
		{
			ReportError(error);
		}
	}

	/// <summary>
	/// Called when the user wants to open a file. Opens a file chooser dialog.
	/// </summary>
	private void OnOpenInsFile()
	{
		try
		{
			var fileChooser = FileChooserDialog.Open(
				"Open Instruction File",
				"Instruction Files",
				"*.ins",
				true,
				false);
			fileChooser.OnFileSelected.ConnectTo(OnNewFromInstructionFile);
			fileChooser.Run();
		}
		catch (Exception error)
		{
			ReportError(error);
		}
	}

	/// <summary>
	/// Called when a close-request signal is received.
	/// </summary>
	/// <param name="sender">Sender object (this).</param>
	/// <param name="args">Event data.</param>
	/// <returns>false</returns>
	private bool OnClosed(Gtk.Window sender, EventArgs args)
	{
		try
		{
			OnClose.Invoke();
		}
		catch (Exception error)
		{
			Console.Error.WriteLine($"Exception thrown in OnClosed callback:");
			Console.Error.WriteLine(error);
		}

		// Returning true prevents other callbacks from executing, which
		// ultimately prevents the window from closing.
		return false;
	}
}
