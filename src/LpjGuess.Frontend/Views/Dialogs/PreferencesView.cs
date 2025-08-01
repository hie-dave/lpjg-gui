using Adw;
using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays configuration options.
/// </summary>
public class PreferencesView : IPreferencesView
{
	/// <summary>
	/// Default window title.
	/// </summary>
	private const string title = "Preferences";

	/// <summary>
	/// The window used to display the properties.
	/// </summary>
	private readonly PreferencesWindow window;

	/// <summary>
	/// The button used to display the 'dark mode' input.
	/// </summary>
	private readonly Switch darkModeButton;

	/// <summary>
	/// The button used to display the 'go to logs tab' input.
	/// </summary>
	private readonly Switch goToLogsTabButton;

	/// <summary>
	/// Preferencse page which displays runner configurations.
	/// </summary>
	private RunnersView runnersPage;

	/// <inheritdoc />
	public Event<bool> DarkModeChanged { get; private init; }

	/// <inheritdoc />
	public Event<bool> GoToLogsTabChanged { get; private init; }

	/// <inheritdoc />
	public Event OnClose { get; private init; }

	/// <inheritdoc />
	public Event OnAddRunner { get; private init; }

	/// <inheritdoc />
	public Event<int> OnDeleteRunner { get; private init; }

	/// <inheritdoc />
	public Event<int> OnEditRunner { get; private init; }

	/// <inheritdoc />
	public Event<int> OnToggleDefaultRunner { get; private init; }

	/// <summary>
	/// A reference to the currently-displayed preferences view, if one is being
	/// displayed.
	/// </summary>
	public static PreferencesWindow? Instance { get; private set; }

	/// <summary>
	/// Create a new <see cref="PreferencesView"/> instance.
	/// </summary>
	/// <param name="darkMode">The initial value of the 'prefer dark mode' property.</param>
	/// <param name="goToLogs">The initial value of the 'go to logs tab' property.</param>
	/// <param name="runnerMetadata">The runners' metadata.</param>
	public PreferencesView(bool darkMode, bool goToLogs, IReadOnlyList<IRunnerMetadata> runnerMetadata)
	{
		darkModeButton = new Switch();
		darkModeButton.Valign = Align.Center;
		goToLogsTabButton = new Switch();
		goToLogsTabButton.Valign = Align.Center;

		ActionRow darkModeRow = new ActionRow();
		darkModeRow.Title = "Prefer dark mode";
		darkModeRow.Subtitle = "True to use dark theme. False to use system theme.";
		darkModeRow.AddSuffix(darkModeButton);

		ActionRow goToLogsTabRow = new ActionRow();
		goToLogsTabRow.Title = "Automatically go to logs tab";
		goToLogsTabRow.Subtitle = "Iff true, the logs tab will automatically be selected when a simulation is run.";
		goToLogsTabRow.AddSuffix(goToLogsTabButton);

		PreferencesGroup generalGroup = new PreferencesGroup();
		generalGroup.Add(darkModeRow);
		generalGroup.Add(goToLogsTabRow);

		PreferencesPage generalPage = new PreferencesPage();
		generalPage.Title = "Settings";
		generalPage.Add(generalGroup);
		generalPage.IconName = Icons.Settings;

		runnersPage = new RunnersView(runnerMetadata);

		window = new PreferencesWindow();
		window.TransientFor = MainView.Instance;
		window.Title = title;
		window.Add(generalPage);
		window.Add(runnersPage);

		DarkModeChanged = new Event<bool>();
		GoToLogsTabChanged = new Event<bool>();
		OnClose = new Event();
		OnAddRunner = new Event();
		OnDeleteRunner = new Event<int>();
		OnEditRunner = new Event<int>();
		OnToggleDefaultRunner = new Event<int>();

		ConnectEvents();

		darkModeButton.State = darkModeButton.Active = darkMode;
		goToLogsTabButton.State = goToLogsTabButton.Active = goToLogs;

		Instance = window;
	}

	/// <inheritdoc />
	public void PopulateRunners(IReadOnlyList<IRunnerMetadata> metadata)
	{
		DisconnectRunnerEvents();
		window.Remove(runnersPage);
		runnersPage.Dispose();
		runnersPage = new RunnersView(metadata);
		window.Add(runnersPage);
		ConnectRunnerEvents();
	}

	/// <inheritdoc />
	public void Show() => window.Present();

	/// <inheritdoc />
	public Widget GetWidget() => window;

	/// <inheritdoc />
	public void Dispose()
	{
		DisconnectEvents();
		OnClose.Dispose();
		runnersPage.Dispose();
		window.Dispose();
	}

	/// <summary>
	/// Connect all event sources to their sinks.
	/// </summary>
	private void ConnectEvents()
	{
		darkModeButton.OnStateSet += OnToggleDarkMode;
		goToLogsTabButton.OnStateSet += OnToggleGoToLogsTab;
		window.OnCloseRequest += OnWindowClosed;

		ConnectRunnerEvents();
	}

	/// <summary>
	/// Disconnect all event sources from their sinks.
	/// </summary>
	private void DisconnectEvents()
	{
		darkModeButton.OnStateSet -= OnToggleDarkMode;
		goToLogsTabButton.OnStateSet -= OnToggleGoToLogsTab;
		window.OnCloseRequest -= OnWindowClosed;
		DarkModeChanged.DisconnectAll();
		OnClose.DisconnectAll();
		DisconnectRunnerEvents();
	}

	/// <summary>
	/// Connect all events on the runner page.
	/// </summary>
	private void ConnectRunnerEvents()
	{
		runnersPage.OnAddRunner.ConnectTo(OnAddRunner);
		runnersPage.OnDelete.ConnectTo(OnDeleteRunner);
		runnersPage.OnEdit.ConnectTo(OnEditRunner);
		runnersPage.OnToggleDefault.ConnectTo(OnToggleDefaultRunner);
	}

	/// <summary>
	/// Disconnect all events from the runner page.
	/// </summary>
	private void DisconnectRunnerEvents()
	{
		runnersPage.OnAddRunner.DisconnectAll();
		runnersPage.OnDelete.DisconnectAll();
		runnersPage.OnEdit.DisconnectAll();
		runnersPage.OnToggleDefault.DisconnectAll();
	}

	/// <summary>
	/// Called when the user has closed the window.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private bool OnWindowClosed(Gtk.Window sender, EventArgs args)
	{
		try
		{
			OnClose.Invoke();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
		return false;
	}

	/// <summary>
	/// Called when the 'prefer dark mode' option is toggled by the user.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private bool OnToggleDarkMode(Switch sender, Switch.StateSetSignalArgs args)
	{
		try
		{
			DarkModeChanged.Invoke(args.State);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
		return false;
	}

	/// <summary>
	/// Called when the 'prefer dark mode' option is toggled by the user.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private bool OnToggleGoToLogsTab(Switch sender, Switch.StateSetSignalArgs args)
	{
		try
		{
			GoToLogsTabChanged.Invoke(args.State);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
		return false;
	}
}
