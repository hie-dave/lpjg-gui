using Adw;
using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views.Rows;

/// <summary>
/// A row in the runners configuration dialog.
/// </summary>
internal class RunnerRow : IDisposable
{
	/// <summary>
	/// Index of this row which will be passed into the callback functions.
	/// </summary>
	private readonly int index;

	/// <summary>
	/// A button which allows the user to edit the runner configuration.
	/// </summary>
	private readonly Button editButton;

	/// <summary>
	/// A button which allows the user to delete the runner.
	/// </summary>
	private readonly Button deleteButton;

	/// <summary>
	/// The 'is default' checkbox.
	/// </summary>
	private readonly CheckButton checkbox;

	/// <summary>
	/// The preferences row object.
	/// </summary>
	public PreferencesRow Row { get; private init; }

	/// <summary>
	/// Called when the user wants to edit this runner.
	/// </summary>
	public Event<int> OnEdit { get; private init; }

	/// <summary>
	/// Called when the user wants to delete this runner.
	/// </summary>
	public Event<int> OnDelete { get; private init; }

	/// <summary>
	/// Called when the user wants to toggle this runner's IsDefault status.
	/// </summary>
	public Event<int> OnDefault { get; private init; }

	/// <summary>
	/// Create a new <see cref="RunnerRow"/> instance.
	/// </summary>
	/// <param name="name">Name of this runner.</param>
	/// <param name="index">Index of this runner which will be passed to the callback functions.</param>
	/// <param name="isDefault">Is this the default runner?</param>
	public RunnerRow(string name, int index, bool isDefault)
	{
		this.index = index;

		OnDelete = new Event<int>();
		OnDefault = new Event<int>();
		OnEdit = new Event<int>();

		editButton = new Button();
		editButton.Valign = Align.Center;
		editButton.IconName = Icons.Edit;

		deleteButton = new Button();
		deleteButton.IconName = Icons.Delete;
		deleteButton.Valign = Align.Center;
		deleteButton.AddCssClass(StyleClasses.DestructiveAction);

		checkbox = new CheckButton();
		checkbox.Active = isDefault;

		ActionRow row = new ActionRow();
		row.Title = name;
		row.AddPrefix(checkbox);
		row.AddSuffix(editButton);
		row.AddSuffix(deleteButton);

		Row = row;

		ConnectEventHandlers();
	}

	/// <summary>
	/// Connect all event handlers.
	/// </summary>
	private void ConnectEventHandlers()
	{
		editButton.OnClicked += OnEditClicked;
		deleteButton.OnClicked += OnDeleteClicked;
		checkbox.OnToggled += OnDefaultToggled;
	}

	/// <summary>
	/// Disconnect all event handlers.
	/// </summary>
	private void DisconnectEventHandlers()
	{
		editButton.OnClicked -= OnEditClicked;
		deleteButton.OnClicked -= OnDeleteClicked;
		checkbox.OnToggled -= OnDefaultToggled;

		OnDelete.DisconnectAll();
		OnEdit.DisconnectAll();
		OnDefault.DisconnectAll();
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		DisconnectEventHandlers();
		Row.Dispose();
	}

	/// <summary>
	/// Called when the user has toggled the 'is default' option.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnDefaultToggled(CheckButton sender, EventArgs args)
	{
		try
		{
			OnDefault.Invoke(index);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Called when the user has clicked the delete button.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnDeleteClicked(Button sender, EventArgs args)
	{
		try
		{
			OnDelete.Invoke(index);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Called when the user has clicked the edit button.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnEditClicked(Button sender, EventArgs args)
	{
		try
		{
			OnEdit.Invoke(index);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}
}
