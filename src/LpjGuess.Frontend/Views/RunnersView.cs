using Adw;
using Gtk;
using LpjGuess.Classes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;
using LpjGuess.Frontend.Views.Rows;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// This class displays a list of <see cref="IRunnerView"/> instances along with
/// options to add or remove runners from the list.
/// </summary>
internal class RunnersView : PreferencesPage
{
	/// <summary>
	/// The runners displayed on this page.
	/// </summary>
	private readonly IReadOnlyList<IRunnerMetadata> metadata;

	/// <summary>
	/// Button used to add new runners to the list of runners.
	/// </summary>
	private readonly Button addRunnerButton;

	/// <summary>
	/// Group containing header title/button.
	/// </summary>
	private readonly PreferencesGroup headerGroup;

	/// <summary>
	/// Group containing the runner rows.
	/// </summary>
	private readonly PreferencesGroup runnersGroup;

	/// <summary>
	/// The runner rows.
	/// </summary>
	private readonly IReadOnlyList<RunnerRow> rows;

	/// <summary>
	/// Called when the user wants to add a runner.
	/// </summary>
	public Event OnAddRunner { get; private init; }

	/// <summary>
	/// Called when the user wants to delete a runner. The argument is the index
	/// of the runner to be deleted.
	/// </summary>
	public Event<int> OnDelete { get; private init; }

	/// <summary>
	/// Called when the user wants to edit a runner. The argument is the index
	/// of the runner to be edit.
	/// </summary>
	public Event<int> OnEdit { get; private init; }

	/// <summary>
	/// Called when the user wants to toggle the IsDefault status of a runner.
	/// </summary>
	public Event<int> OnToggleDefault { get; private init; }

	/// <summary>
	/// Create a new <see cref="RunnersView"/> instances.
	/// </summary>
	/// <param name="metadata">The runners displayed by this view.</param>
	public RunnersView(IReadOnlyList<IRunnerMetadata> metadata)
	{
		this.metadata = metadata;
		OnAddRunner = new Event();
		OnDelete = new Event<int>();
		OnEdit = new Event<int>();
		OnToggleDefault = new Event<int>();

		Title = "Runners";
		IconName = Icons.RunConfig;

		addRunnerButton = Button.NewWithLabel("Add");
		addRunnerButton.IconName = Icons.AddItem;

		headerGroup = new PreferencesGroup();
		headerGroup.Title = "Runners";
		headerGroup.HeaderSuffix = addRunnerButton;
		Add(headerGroup);

		runnersGroup = new PreferencesGroup();
		List<RunnerRow> rows = new List<RunnerRow>();
		for (int i = 0; i < metadata.Count; i++)
		{
			IRunnerMetadata meta = metadata[i];
			RunnerRow row = new RunnerRow(meta.Name, meta.IsDefault);
			rows.Add(row);
			runnersGroup.Add(row.Row);

			row.OnEdit.ConnectTo(() => OnEdit.Invoke(i));
			row.OnDelete.ConnectTo(() => OnDelete.Invoke(i));
			row.OnDefault.ConnectTo(() => OnToggleDefault.Invoke(i));
		}
		this.rows = rows;

		ConnectEvents();
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public override void Dispose()
	{
		DisconnectEvents();

		addRunnerButton.Dispose();
		headerGroup.Dispose();
		runnersGroup.Dispose();

		foreach (RunnerRow row in rows)
			row.Dispose();

		base.Dispose();
	}

	/// <summary>
	/// Connect all event sources to their sinks.
	/// </summary>
	private void ConnectEvents()
	{
		addRunnerButton.OnClicked += OnAddClicked;
	}

	/// <summary>
	/// Disconnect all event sources from their sinks.
	/// </summary>
	private void DisconnectEvents()
	{
		addRunnerButton.OnClicked -= OnAddClicked;

		OnAddRunner.DisconnectAll();
		OnDelete.DisconnectAll();
		OnEdit.DisconnectAll();
		OnToggleDefault.DisconnectAll();
	}

	/// <summary>
	/// Called when the user wants to add a new runner.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnAddClicked(Button sender, EventArgs args)
	{
		try
		{
			OnAddRunner.Invoke();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}
}
