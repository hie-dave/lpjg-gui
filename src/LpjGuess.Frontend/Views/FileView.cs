using System.Text;
using Gio;
using Gtk;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;

using File = System.IO.File;
using Action = System.Action;
using LpjGuess.Frontend.Utility.Gtk;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays the contents of an instruction file to the user, along
/// with controls for running the file.
/// </summary>
public class FileView : Box, IFileView
{
	/// <summary>
	/// Spacing between internal widgets (in px).
	/// </summary>
	private const int spacing = 5;

	/// <summary>
	/// Domain for file-specific actions.
	/// </summary>
	private const string actionDomain = "file";

	/// <summary>
	/// Input modules.
	/// </summary>
	private static readonly string[] inputModules = new string[]
	{
		"nc",
		"nc",
		"site",
		"cru",
		"fluxnet",
	};

	/// <summary>
	/// Full path and filename of the file being displayed.
	/// </summary>
	private readonly string fileName;

	/// <summary>
	/// Action to be invoked when the user clicks 'run'.
	/// </summary>
	private readonly Action onRun;

	/// <summary>
	/// Action to be invoked when the user clicks 'stop'.
	/// </summary>
	private readonly Action onStop;

	/// <summary>
	/// Called when the user wants to add a new run option.
	/// </summary>
	private readonly Action onAddRunOption;

	/// <summary>
	/// The run button.
	/// </summary>
	private readonly Button run;

	/// <summary>
	/// The stop button.
	/// </summary>
	private readonly Button stop;

	/// <summary>
	/// Dropdown containing the input modules.
	/// </summary>
	private readonly DropDown inputModuleDropdown;

	/// <summary>
	/// Notebook containing tabs for .ins files, guess output, etc.
	/// </summary>
	private readonly Notebook notebook;

	/// <summary>
	/// TextView containing output from the child lpj-guess process.
	/// </summary>
	private readonly TextView logs;

	/// <summary>
	/// ScrolledWindow containing the lpj-guess output TextView widget.
	/// </summary>
	private readonly ScrolledWindow logsScroller;

	/// <summary>
	/// A view which allows the user to browse the raw outputs from the model.
	/// </summary>
	private readonly OutputsView outputsView;

	/// <summary>
	/// A view which displays configurable graphs.
	/// </summary>
	private readonly GraphsView graphsView;

	/// <summary>
	/// The TextView API is currently quite limited, so I'm using a separate
	/// StringBuilder to track the contents of the output TextView.
	/// </summary>
	private readonly StringBuilder outputContents;

	/// <summary>
	/// The run options menu.
	/// </summary>
	private readonly Menu runMenu;

	/// <summary>
	/// Create a new <see cref="FileView"/> instance for a particular .ins file.
	/// </summary>
	/// <param name="fileName">Full path/name of the file.</param>
	/// <param name="onRun">Called when the user wants to run the file.</param>
	/// <param name="onStop">Called when the user wants to cancel a running simulation.</param>
	/// <param name="onAddRunOption">Called when the user wants to add a new run method.</param>
	public FileView(string fileName, Action onRun, Action onStop, Action onAddRunOption) : base()
	{
		this.fileName = fileName;
		this.onRun = onRun;
		this.onStop = onStop;
		this.onAddRunOption = onAddRunOption;

		SetOrientation(Orientation.Vertical);
		Spacing = spacing;

		// Create a TextView widget to display file contents.
		// todo: gtksourceview. But this requires bindings...
		TextBuffer buffer = TextBuffer.New(null);
		buffer.Text = File.ReadAllText(fileName);
		TextView text = TextView.NewWithBuffer(buffer);
		text.Vexpand = true;
		text.Monospace = true;

		outputContents = new StringBuilder();

		ScrolledWindow scroller = new ScrolledWindow();
		scroller.Child = text;

		inputModuleDropdown = DropDown.NewFromStrings(inputModules);
		inputModuleDropdown.Hexpand = true;

		Box inputModuleBox = new Box();
		inputModuleBox.SetOrientation(Orientation.Horizontal);
		inputModuleBox.Spacing = spacing;
		inputModuleBox.Append(Label.New("Input Module: "));
		inputModuleBox.Append(inputModuleDropdown);

		// Create a run button.
		run = Button.NewWithLabel("Run");
		run.AddCssClass(StyleClasses.SuggestedAction);
		run.Hexpand = true;
		run.Name = "run-btn";

		runMenu = Menu.New();

		MenuButton runOpts = new MenuButton();
		runOpts.AddCssClass(StyleClasses.SuggestedAction);
		runOpts.Name = "run-opts";
		runOpts.MenuModel = runMenu;

		Box runBox = new Box();
		runBox.SetOrientation(Orientation.Horizontal);
		runBox.Append(run);
		runBox.Append(runOpts);

		// Create a stop button.
		stop = Button.NewWithLabel("Stop");
		stop.AddCssClass(StyleClasses.DestructiveAction);
		stop.Visible = false;

		logs = new TextView();
		logs.Monospace = true;
		logsScroller = new ScrolledWindow();
		logsScroller.Child = logs;

		outputsView = new OutputsView();
		graphsView = new GraphsView();

		notebook = new Notebook();
		notebook.AppendPage(scroller, Label.New("Instruction File"));
		notebook.AppendPage(logsScroller, Label.New("Logs"));
		notebook.AppendPage(outputsView, Label.New("Outputs"));
		notebook.AppendPage(graphsView, Label.New("Graphs"));
		// notebook.ShowTabs = false;

		Append(notebook);
		Append(inputModuleBox);
		Append(runBox);
		Append(stop);

		ConnectEvents();
	}

	/// <summary>
	/// Currently-selected input module.
	/// </summary>
	public string InputModule
	{
		get
		{
			// Cast here should be safe...I think.
			int selectedIndex = (int)inputModuleDropdown.Selected;
			if (selectedIndex < inputModules.Length)
				return inputModules[selectedIndex];
			throw new Exception($"No input module is selected");
		}
	}

	/// <inheritdoc />
	public IGraphsView GraphsView => graphsView;

	/// <inheritdoc />
	public Widget GetWidget() => this;

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public override void Dispose()
	{
		DisconnectEvents();
		base.Dispose();
	}

	/// <inheritdoc />
	public void AppendOutput(string stdout)
	{
		lock (outputContents)
			outputContents.AppendLine(stdout);
		GLib.Functions.IdleAddFull(0, _ =>
		{
			Adjustment? adj = logsScroller.Vadjustment;
			double scroll = adj?.Value ?? 0;
			string text;
			lock (outputContents)
				text = outputContents.ToString();
			logs.Show();
			logs.GetBuffer().SetText(text, Encoding.UTF8.GetByteCount(text));
			// If scrolled window is at bottom of screen, scroll to bottom.
			// Otherwise, scroll to previous scroll position. This should be
			// refactored once the gircore API includes TextIter methods.
			if (adj != null)
			{
				if (scroll >= (adj.Upper - adj.PageSize))
					scroll = adj.Upper;
				Console.WriteLine($"Setting vadj to {scroll} (pos = {adj.Value}, last page cutoff = {adj.Upper - adj.PageSize})");
				adj.Value = scroll;
			}
			return false;
		});
	}

	/// <inheritdoc />
	public void AppendError(string stderr)
	{
		// tbi
		AppendOutput(stderr);
	}

	/// <inheritdoc />
	public void ClearOutput()
	{
		outputContents.Clear();
		logs.GetBuffer().SetText("", 0);
	}

	/// <inheritdoc />
	public void ShowRunButton(bool show)
	{
		run.Visible = show;
		stop.Visible = !show;
	}

	/// <summary>
	/// Connect widgets to event callbacks.
	/// </summary>
	private void ConnectEvents()
	{
		run.OnClicked += Run;
		stop.OnClicked += Stop;
	}

	/// <summary>
	/// Disconnect widgets from event callbacks.
	/// </summary>
	private void DisconnectEvents()
	{
		run.OnClicked -= Run;
		stop.OnClicked -= Stop;
	}

	private void AddMenuItem(string name, Action callback)
	{
		runMenu.AddMenuItem(actionDomain, name, callback);
	}

	/// <summary>
	/// Called when the run button is clicked.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void Run(object sender, EventArgs args)
	{
		try
		{
			onRun();
			run.Hide();
			stop.Show();
			logs.Visible = true;
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Called when the "stop" button is clicked.
	/// </summary>
	/// <param name="sender">Sener object.</param>
	/// <param name="args">Event data.</param>
	private void Stop(object sender, EventArgs args)
	{
		try
		{
			onStop();
			stop.Hide();
			run.Show();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}
}
