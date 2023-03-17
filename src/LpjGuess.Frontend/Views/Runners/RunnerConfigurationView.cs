using Adw;
using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views.Runners;

/// <summary>
/// A dialog which displays configuration settings for a runner configuration.
/// </summary>
public class RunnerConfigurationView : IRunnerView
{
	/// <summary>
	/// The window object.
	/// </summary>
	private readonly PreferencesWindow window;

	/// <summary>
	/// Row containing the editable runner name widget.
	/// </summary>
	private readonly EntryRow nameRow;

	/// <summary>
	/// A switch widget which allows the user to toggle the 'is default' option
	/// for this runner.
	/// </summary>
	private readonly Switch isDefaultSwitch;

	/// <summary>
	/// Called when the window is closed by the user.
	/// </summary>
	public Event OnClose { get; private init; }

	/// <summary>
	/// Called when the user toggles the "is default" option.
	/// </summary>
	public Event<bool> OnToggleDefault { get; private init; }

	/// <summary>
	/// Called when the user changes the name of the runner.
	/// </summary>
	public Event<string> OnChangeName { get; private init; }

	/// <summary>
	/// Create a new <see cref="RunnerConfigurationView"/> instance.
	/// </summary>
	/// <param name="metadata">Runner metadata.</param>
	/// <param name="configGroup">Group containing config options for this runner.</param>
	public RunnerConfigurationView(IRunnerMetadata metadata, IGroupView configGroup)
	{
		OnToggleDefault = new Event<bool>();
		OnChangeName = new Event<string>();
		OnClose = new Event();

		nameRow = new EntryRow();
		nameRow.Title = "Name";
		nameRow.SetText(metadata.Name);
		nameRow.ShowApplyButton = true;

		isDefaultSwitch = new Switch();
		isDefaultSwitch.Valign = Align.Center;
		isDefaultSwitch.Active = metadata.IsDefault;

		ActionRow isDefaultRow = new ActionRow();
		isDefaultRow.Title = "Default";
		isDefaultRow.Subtitle = "If true, this runner will be used when the run button is clicked without selecting a specific runner";
		isDefaultRow.AddSuffix(isDefaultSwitch);

		PreferencesGroup standardGroup = new PreferencesGroup();
		standardGroup.Add(nameRow);
		standardGroup.Add(isDefaultRow);

		PreferencesPage page = new PreferencesPage();
		page.Add(standardGroup);
		page.Add(configGroup.GetGroup());

		window = new PreferencesWindow();
		window.TransientFor = PreferencesView.Instance as Gtk.Window ?? MainView.Instance;
		window.Title = $"{metadata.Name} Configuration";
		window.Add(page);

		ConnectEvents();
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		DisconnectEvents();
		window.Dispose();
	}

	/// <summary>
	/// Show the view.
	/// </summary>
	public void Show()
	{
		window.Present();
	}

	/// <inheritdoc />
	public Widget GetWidget() => window;

	/// <summary>
	/// Connect event handlers.
	/// </summary>
	private void ConnectEvents()
	{
		window.OnCloseRequest += OnWindowClosed;
		isDefaultSwitch.OnStateSet += OnToggleIsDefault;
		nameRow.OnApply += OnNameChanged;
	}

	/// <summary>
	/// Disconnect event handlers.
	/// </summary>
	private void DisconnectEvents()
	{
		window.OnCloseRequest -= OnWindowClosed;
		isDefaultSwitch.OnStateSet += OnToggleIsDefault;
		nameRow.OnApply += OnNameChanged;

		OnChangeName.DisconnectAll();
		OnToggleDefault.DisconnectAll();
	}

	/// <summary>
	/// Called when the user changes the runner name.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnNameChanged(EntryRow sender, EventArgs args)
	{
		try
		{
			OnChangeName.Invoke(nameRow.GetText());
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Called when the user toggles the 'is default' option.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnToggleIsDefault(Switch sender, Switch.StateSetSignalArgs args)
	{
		try
		{
			OnToggleDefault.Invoke(args.State);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	private void OnWindowClosed(Gtk.Window sender, EventArgs args)
	{
		try
		{
			OnClose.Invoke();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}
}
