using System.Text;
using Gio;
using Gtk;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;

using File = System.IO.File;
using Action = System.Action;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;
using LpjGuess.Frontend.Enumerations;
using LpjGuess.Runner.Models;

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
	/// Name of an action which represents a request to add a runner.
	/// </summary>
	private const string addRunnerAction = "Add Runner";

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
	private readonly IReadOnlyList<string> fileNames;

	/// <summary>
	/// The run button.
	/// </summary>
	private readonly Button run;

	/// <summary>
	/// Box containing the run button.
	/// </summary>
	private readonly Box runBox;

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
	/// ScrolledWindow containing the lpj-guess output TextView widget.
	/// </summary>
	private readonly ScrolledWindow logsScroller;

	/// <summary>
	/// A view which displays the contents of an instruction file.
	/// </summary>
	private readonly IEditorView insFileView;

	/// <summary>
	/// A view which allows the user to browse the raw outputs from the model.
	/// </summary>
	private readonly OutputsView outputsView;

	/// <summary>
	/// A view which displays configurable graphs.
	/// </summary>
	private readonly GraphsView graphsView;

	/// <summary>
	/// The run options menu.
	/// </summary>
	private readonly Menu runMenu;

	/// <summary>
	/// A progress bar to display the progress of a currently-running simulation.
	/// </summary>
	private readonly ProgressBar progressBar;

	/// <inheritdoc />
	public Event<string?> OnRun { get; private init; }

	/// <inheritdoc />
	public Event OnStop { get; private init; }

	/// <inheritdoc />
	public Event OnAddRunOption { get; private init; }

	/// <summary>
	/// Create a new <see cref="FileView"/> instance for a particular .ins file.
	/// </summary>
	/// <param name="fileNames">Full path/name of the file.</param>
	public FileView(IEnumerable<string> fileNames) : base()
	{
		this.fileNames = fileNames.ToList();
		OnRun = new Event<string?>();
		OnStop = new Event();
		OnAddRunOption = new Event();

		SetOrientation(Orientation.Vertical);
		Spacing = spacing;

		// Create a TextView widget to display file contents.
		// todo: gtksourceview. But this requires bindings...
		insFileView = new EditorView();
		insFileView.Editable = true;
		insFileView.AppendLine(File.ReadAllText(fileNames));
		ScrolledWindow scroller = new ScrolledWindow();
		scroller.Child = insFileView.GetWidget();

		inputModuleDropdown = DropDown.NewFromStrings(inputModules);
		inputModuleDropdown.Hexpand = true;

		Box inputModuleBox = new Box();
		inputModuleBox.SetOrientation(Orientation.Horizontal);
		inputModuleBox.Spacing = spacing;
		inputModuleBox.Append(Label.New("Input Module: "));
		inputModuleBox.Append(inputModuleDropdown);

		// Create a run button.
		// todo: replace with SplitButton.
		run = Button.NewWithLabel("Run");
		run.AddCssClass(StyleClasses.SuggestedAction);
		run.Hexpand = true;
		run.Name = "run-btn";

		runMenu = Menu.New();

		MenuButton runOpts = new MenuButton();
		runOpts.AddCssClass(StyleClasses.SuggestedAction);
		runOpts.Name = "run-opts";
		runOpts.MenuModel = runMenu;

		runBox = new Box();
		runBox.SetOrientation(Orientation.Horizontal);
		runBox.Append(run);
		runBox.Append(runOpts);

		// Create a stop button.
		stop = Button.NewWithLabel("Stop");
		stop.AddCssClass(StyleClasses.DestructiveAction);
		stop.Visible = false;

		LogsView = new EditorView();
		LogsView.Editable = false;
		logsScroller = new ScrolledWindow();
		logsScroller.Child = LogsView.GetWidget();

		outputsView = new OutputsView();
		graphsView = new GraphsView();

		notebook = new Notebook();
		notebook.AppendPage(scroller, Label.New("Instruction File"));
		notebook.AppendPage(logsScroller, Label.New("Logs"));
		notebook.AppendPage(outputsView, Label.New("Outputs"));
		notebook.AppendPage(graphsView, Label.New("Graphs"));
		// notebook.ShowTabs = false;

		progressBar = new ProgressBar();
		progressBar.ShowText = true;
		progressBar.Halign = Align.Fill;
		progressBar.Valign = Align.End;
		progressBar.Visible = false;
		Append(notebook);
		Append(inputModuleBox);
		Append(runBox);
		Append(stop);
		Append(progressBar);

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
	public IEditorView LogsView { get; private init; }

	/// <inheritdoc />
	public Widget GetWidget() => this;

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public override void Dispose()
	{
		DisconnectEvents();
		runMenu.Dispose();
		base.Dispose();
	}

	/// <inheritdoc />
	public void AppendTab(string name, IView view)
	{
		notebook.AppendPage(view.GetWidget(), Label.New(name));
	}

	/// <inheritdoc />
	public void AppendOutput(string stdout)
	{
		MainView.RunOnMainThread(() =>
		{
			Adjustment? adj = logsScroller.Vadjustment;
			double scroll = adj?.Value ?? 0;
			LogsView.GetWidget().Show();
			LogsView.AppendLine(stdout);
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
		LogsView.Clear();
	}

	/// <inheritdoc />
	public void ShowRunButton(bool show)
	{
		runBox.Visible = show;
		stop.Visible = !show;
	}

	/// <inheritdoc />
	public void SelectTab(FileTab tab)
	{
		notebook.SetCurrentPage((int)tab);
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
		ClearRunOptions();
		run.OnClicked -= Run;
		stop.OnClicked -= Stop;
		OnRun.DisconnectAll();
		OnStop.DisconnectAll();
		OnAddRunOption.DisconnectAll();
	}

	/// <summary>
	/// Remove all options from the runners dropdown.
	/// </summary>
	public void ClearRunOptions()
	{
		runMenu.RemoveAll();
	}

	/// <inheritdoc />
	public void SetRunners(IEnumerable<string> runners)
	{
		ClearRunOptions();
		foreach (string name in runners)
			runMenu.AddMenuItem(actionDomain, name, OnRunWithRunner);
		runMenu.AddMenuItem(actionDomain, addRunnerAction, OnAddRunoption);
	}

	/// <summary>
	/// Called when the user wants to add a runner.
	/// </summary>
	private void OnAddRunoption()
	{
		try
		{
			OnAddRunOption.Invoke();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// User wants to run the file with a particular runner.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnRunWithRunner(SimpleAction sender, SimpleAction.ActivateSignalArgs args)
	{
		try
		{
			string? name = sender.Name;
			if (name != null)
				OnRun.Invoke(name);
			runBox.Hide();
			stop.Show();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
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
			OnRun.Invoke(null);
			runBox.Hide();
			stop.Show();
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
			OnStop.Invoke();
			stop.Hide();
			runBox.Show();
			progressBar.Visible = false;
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

    /// <summary>
    /// Show the progress of a currently-running simulation.
    /// </summary>
    /// <param name="progress">Current simulation progress.</param>
    public void ShowProgress(double progress)
    {
        progressBar.Fraction = progress;
        progressBar.Text = $"Progress: {progress:P0}";
        progressBar.Visible = (progress > 0 && progress < 1;
    }
}
