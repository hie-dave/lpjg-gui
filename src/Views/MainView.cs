using System.Text;
using Adw;
using Gio;
using Gtk;
using LpjGuess.Frontend.EventTypes;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Utility;

// Application is ambiguous between Adw, Gio, and Gtk.
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
	/// Default window width.
	/// </summary>
	private const int defaultWidth = 640;

	/// <summary>
	/// Default window height.
	/// </summary>
	private const int defaultHeight = 480;

	/// <summary>
	/// Margin (in px) around window contents and winow border.
	/// </summary>
	private const int margin = 8;

	/// <summary>
	/// Spacing (in px) between internal elements of the window.
	/// </summary>
	private const int spacing = 5;

	/// <summary>
	/// The application object.
	/// </summary>
	private readonly Application app;

	/// <summary>
	/// The application menu widget. Actions added via
	/// <see cref="AddMenuItem(string, Event, string?)"/> will be added here.
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
	public event Event<string>? OpenFile;

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="app"></param>
	public MainView(Application app) : base()
	{
		this.app = app;

		DefaultWidth = defaultWidth;
		DefaultHeight = defaultHeight;
		SetApplication(app);

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
		titleBox.Orientation = Orientation.Vertical;
		titleBox.Valign = Align.Center;
		titleBox.Append(title);
		titleBox.Append(subtitle);

		var header = new Adw.HeaderBar();
		header.CenteringPolicy = CenteringPolicy.Strict;
		header.TitleWidget = titleBox;
		header.PackStart(openInsFile);
		header.PackEnd(menuButton);

		main = new Box();
		main.Orientation = Orientation.Vertical;
		main.MarginTop = margin;
		main.MarginBottom = margin;
		main.MarginStart = margin;
		main.MarginEnd = margin;

		Box contents = new Box();
		contents.SetOrientation(Orientation.Vertical);
		contents.Append(header);
		contents.Append(main);
		SetContent(contents);
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
	public void AddMenuItem(string name, Event callback, string? hotkey = null)
	{
		const string domain = "app";
		string actionName = name.ToLower();
		string fullName = $"{domain}.{actionName}";

		menu.Append($"_{name}", fullName);
		var action = SimpleAction.New(actionName, null);
		action.OnActivate += (_, __) => callback();
		if (hotkey != null)
			app.SetAccelsForAction(fullName, new[] { hotkey });
		app.AddAction(action);
	}

	/// <summary>
	/// Error handler routine.
	/// </summary>
	/// <param name="error"></param>
	public void ReportError(Exception error)
	{
		// Show error in a dialog box for now.
		var dialog = new MessageDialog();
		dialog.Modal = true;
		dialog.TransientFor = this;
		dialog.Body = error.ToString();
		dialog.Title = "Error";
		dialog.AddResponse("ok", "_Ok");
		// dialog.SetDefaultResponse("ok");
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
			FileChooserNative fileChooser = FileChooserNative.New(
				"Open Instruction File",
				this,
				FileChooserAction.Open,
				"Open",
				"Cancel"
			);

			FileFilter filterIns = FileFilter.New();
			filterIns.AddPattern("*.ins");
			filterIns.Name = "Instruction Files (*.ins)";

			FileFilter filterAll = FileFilter.New();
			filterAll.AddPattern("*");
			filterAll.Name = "All Files";

			fileChooser.AddFilter(filterIns);
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
		}
		catch (Exception error)
		{
			ReportError(error);
		}
	}
}
