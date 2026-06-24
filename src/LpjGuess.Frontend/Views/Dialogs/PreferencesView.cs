using Adw;
using Gtk;
using LpjGuess.Core.Models;
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
	private readonly Entry previewColumnLimitEntry;
	private readonly EntryCommitter previewColumnLimitCommitter;
	private readonly StringDropDownView defaultInputModuleDropDown;
	private readonly StringDropDownView<ExistingOutputPolicy> defaultExistingOutputPolicyDropDown;

	private static readonly string[] inputModules =
	[
		"nc",
		"site",
		"cru",
		"fluxnet"
	];

	private static readonly ExistingOutputPolicy[] existingOutputPolicies =
	[
		ExistingOutputPolicy.Preserve,
		ExistingOutputPolicy.CleanManaged,
		ExistingOutputPolicy.PruneStale,
		ExistingOutputPolicy.CleanManaged | ExistingOutputPolicy.PruneStale,
		ExistingOutputPolicy.Fail
	];

	/// <summary>
	/// Preferencse page which displays runner configurations.
	/// </summary>
	private RunnersView runnersPage;

	/// <inheritdoc />
	public Event<bool> DarkModeChanged { get; private init; }

	/// <inheritdoc />
	public Event<bool> GoToLogsTabChanged { get; private init; }

	/// <inheritdoc />
	public Event<int> SimulationPreviewParameterColumnLimitChanged { get; private init; }

	/// <inheritdoc />
	public Event<string> DefaultInputModuleChanged { get; private init; }

	/// <inheritdoc />
	public Event<ExistingOutputPolicy> DefaultExistingOutputPolicyChanged { get; private init; }

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
	/// <param name="previewParameterColumnLimit">Maximum dedicated parameter columns in experiment previews.</param>
	/// <param name="defaultInputModule">Default input module for new experiments.</param>
	/// <param name="defaultExistingOutputPolicy">Default existing-output policy for new experiments.</param>
	/// <param name="runnerMetadata">The runners' metadata.</param>
	public PreferencesView(
		bool darkMode,
		bool goToLogs,
		int previewParameterColumnLimit,
		string defaultInputModule,
		ExistingOutputPolicy defaultExistingOutputPolicy,
		IReadOnlyList<IRunnerMetadata> runnerMetadata)
	{
		darkModeButton = new Switch();
		darkModeButton.Valign = Align.Center;
		goToLogsTabButton = new Switch();
		goToLogsTabButton.Valign = Align.Center;
		previewColumnLimitEntry = new Entry()
		{
			InputPurpose = InputPurpose.Digits,
			MaxWidthChars = 4,
			WidthChars = 4,
			Halign = Align.End
		};
		previewColumnLimitEntry.SetText(Math.Max(0, previewParameterColumnLimit).ToString());
		previewColumnLimitCommitter = new EntryCommitter(
			previewColumnLimitEntry,
			OnPreviewColumnLimitChanged,
			ValidatePreviewColumnLimit);
		defaultInputModuleDropDown = new StringDropDownView();
		defaultInputModuleDropDown.Populate(inputModules);
		defaultInputModuleDropDown.Select(defaultInputModule);
		defaultExistingOutputPolicyDropDown =
			new StringDropDownView<ExistingOutputPolicy>(GetExistingOutputPolicyLabel);
		defaultExistingOutputPolicyDropDown.Populate(existingOutputPolicies);
		defaultExistingOutputPolicyDropDown.Select(defaultExistingOutputPolicy);

		ActionRow darkModeRow = new ActionRow();
		darkModeRow.Title = "Prefer dark mode";
		darkModeRow.Subtitle = "True to use dark theme. False to use system theme.";
		darkModeRow.AddSuffix(darkModeButton);

		ActionRow goToLogsTabRow = new ActionRow();
		goToLogsTabRow.Title = "Automatically go to logs tab";
		goToLogsTabRow.Subtitle = "Iff true, the logs tab will automatically be selected when a simulation is run.";
		goToLogsTabRow.AddSuffix(goToLogsTabButton);

		ActionRow previewColumnLimitRow = new ActionRow();
		previewColumnLimitRow.Title = "Preview parameter-column limit";
		previewColumnLimitRow.Subtitle =
			"Use dedicated parameter columns up to this count. Set to 0 to always group changes.";
		previewColumnLimitRow.AddSuffix(previewColumnLimitEntry);

		ActionRow inputModuleRow = new ActionRow();
		inputModuleRow.Title = "Default input module";
		inputModuleRow.Subtitle = "Used when a new experiment is created.";
		inputModuleRow.AddSuffix(defaultInputModuleDropDown.GetWidget());

		ActionRow existingOutputPolicyRow = new ActionRow();
		existingOutputPolicyRow.Title = "Default existing-output policy";
		existingOutputPolicyRow.Subtitle = "Used when a new experiment is created.";
		existingOutputPolicyRow.AddSuffix(defaultExistingOutputPolicyDropDown.GetWidget());

		PreferencesGroup generalGroup = new PreferencesGroup();
		generalGroup.Add(darkModeRow);
		generalGroup.Add(goToLogsTabRow);
		generalGroup.Add(previewColumnLimitRow);
		generalGroup.Add(inputModuleRow);
		generalGroup.Add(existingOutputPolicyRow);

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
		SimulationPreviewParameterColumnLimitChanged = new Event<int>();
		DefaultInputModuleChanged = new Event<string>();
		DefaultExistingOutputPolicyChanged = new Event<ExistingOutputPolicy>();
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
		previewColumnLimitCommitter.Dispose();
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
		defaultInputModuleDropDown.OnSelectionChanged.ConnectTo(OnDefaultInputModuleChanged);
		defaultExistingOutputPolicyDropDown.OnSelectionChanged.ConnectTo(OnDefaultExistingOutputPolicyChanged);
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
		defaultInputModuleDropDown.OnSelectionChanged.DisconnectFrom(OnDefaultInputModuleChanged);
		defaultExistingOutputPolicyDropDown.OnSelectionChanged.DisconnectFrom(OnDefaultExistingOutputPolicyChanged);
		window.OnCloseRequest -= OnWindowClosed;
		DarkModeChanged.DisconnectAll();
		GoToLogsTabChanged.DisconnectAll();
		SimulationPreviewParameterColumnLimitChanged.DisconnectAll();
		DefaultInputModuleChanged.DisconnectAll();
		DefaultExistingOutputPolicyChanged.DisconnectAll();
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

	private void OnPreviewColumnLimitChanged(string value)
	{
		SimulationPreviewParameterColumnLimitChanged.Invoke(int.Parse(value));
	}

	private void OnDefaultInputModuleChanged(string inputModule)
	{
		DefaultInputModuleChanged.Invoke(inputModule);
	}

	private void OnDefaultExistingOutputPolicyChanged(ExistingOutputPolicy policy)
	{
		DefaultExistingOutputPolicyChanged.Invoke(policy);
	}

	private static string? ValidatePreviewColumnLimit(string value)
	{
		return int.TryParse(value, out int limit) && limit >= 0
			? null
			: "Column limit must be a whole number of zero or greater.";
	}

	private static string GetExistingOutputPolicyLabel(ExistingOutputPolicy policy)
	{
		return policy switch
		{
			ExistingOutputPolicy.Preserve => "Preserve existing outputs",
			ExistingOutputPolicy.CleanManaged => "Clean rerun simulations",
			ExistingOutputPolicy.PruneStale => "Remove stale simulations",
			ExistingOutputPolicy.CleanManaged | ExistingOutputPolicy.PruneStale => "Clean and remove stale simulations",
			ExistingOutputPolicy.Fail => "Fail if outputs exist",
			_ => policy.ToConfigString()
		};
	}
}
