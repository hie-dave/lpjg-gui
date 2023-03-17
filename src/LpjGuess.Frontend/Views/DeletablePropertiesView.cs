using Adw;
using Gtk;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A deletable group of properties. Typically this is used within a list
/// property to allow users to delete an element of a list property.
/// </summary>
public class DeletablePropertiesView : PropertiesView
{
	/// <summary>
	/// Function to be called when the delete button is clicked.
	/// </summary>
	private readonly Action<PreferencesGroup> onDelete;

	/// <summary>
	/// The row containing the delete button.
	/// </summary>
	private readonly ActionRow deleteRow;

	/// <summary>
	/// Create a new <see cref="DeletablePropertiesView"/> instance.
	/// </summary>
	/// <param name="name">Name of the group.</param>
	/// <param name="properties">Properties in the group.</param>
	/// <param name="onDelete">Function to be called when the delete button is clicked.</param>
	public DeletablePropertiesView(string? name, IEnumerable<IPropertyView> properties,
		Action<PreferencesGroup> onDelete) : base(name, properties)
	{
		this.onDelete = onDelete;

		deleteRow = new ActionRow();
		deleteRow.Title = "Delete";
		deleteRow.AddPrefix(Image.NewFromIconName(Icons.Delete));
		deleteRow.Activatable = true;
		ConnectEventHandlers();
		Add(deleteRow);
	}

	/// <inheritdoc />
	public override void Dispose()
	{
		DisconnectEventHandlers();
		base.Dispose();
	}

	/// <summary>
	/// Connect event sinks to sources.
	/// </summary>
	private void ConnectEventHandlers()
	{
		deleteRow.OnActivate += OnDelete;
	}

	/// <summary>
	/// Disconnect event sinks from sources.
	/// </summary>
	private void DisconnectEventHandlers()
	{
		deleteRow.OnActivate -= OnDelete;
	}

	/// <summary>
	/// Called when the user wants to delete this group.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnDelete(ListBoxRow sender, EventArgs args)
	{
		try
		{
			onDelete(this);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}
}
