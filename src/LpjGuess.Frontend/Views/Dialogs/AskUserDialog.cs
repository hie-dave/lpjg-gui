using Adw;
using Gtk;
using Window = Adw.Window;
using HeaderBar = Adw.HeaderBar;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views.Dialogs;

/// <summary>
/// A class which presents the user with a list of options and asks them to
/// select one.
/// </summary>
/// <remarks>
/// Whole lot of clunky here. Don't try and reuse an instance of this class.
/// Just create a new one each time.
/// </remarks>
internal class AskUserDialog : Window
{
	/// <summary>
	/// Margin between contents and window border.
	/// </summary>
	private const int margin = 12;

	/// <summary>
	/// The cancel button.
	/// </summary>
	private readonly Button cancelButton;

	/// <summary>
	/// Function to be called when the select button is clicked. The selected
	/// option will be returned.
	/// </summary>
	private readonly Action<string> onSelected;

	/// <summary>
	/// Action rows.
	/// </summary>
	private readonly IReadOnlyList<ActionRow> actionRows;

	/// <summary>
	/// Create a new <see cref="AskUserDialog"/> instance.
	/// </summary>
	/// <param name="prompt">Prompt to the user (displayed as window title).</param>
	/// <param name="acceptButtonText">Text to go on the 'accept' button.</param>
	/// <param name="options">Valid options.</param>
	/// <param name="onSelected">Function to be called when the select button is clicked.</param>
	public AskUserDialog(string prompt, string acceptButtonText, IEnumerable<string> options, Action<string> onSelected)
	{
		this.onSelected = onSelected;
		Modal = true;
		TransientFor = MainView.Instance;
		Title = "Preferences";
		HideOnClose = true;

		cancelButton = Button.NewWithLabel("Cancel");
		cancelButton.AddCssClass(StyleClasses.DestructiveAction);

		HeaderBar header = new HeaderBar();
		header.CenteringPolicy = CenteringPolicy.Strict;
		header.TitleWidget = Label.New(prompt);
		header.PackStart(cancelButton);

		ListBox choicesBox = new ListBox();
		List<ActionRow> rows = new List<ActionRow>();
		foreach (string option in options)
		{
			ActionRow row = new ActionRow();
			row.Title = option;
			choicesBox.Append(row);
			rows.Add(row);
		}
		actionRows = rows;

		ScrolledWindow choices = new ScrolledWindow();
		choices.MarginBottom = margin;
		choices.MarginTop = margin;
		choices.MarginStart = margin;
		choices.MarginEnd = margin;
		choices.Child = choicesBox;

		Box main = new Box();
		main.Orientation = Orientation.Vertical;
		main.Append(header);
		main.Append(choices);

		ConnectEvents();
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public override void Dispose()
	{
		DisconnectEvents();
		base.Dispose();
	}

	/// <summary>
	/// Run the dialog. Non-blocking. The result is obtained via the callback
	/// passed into the constructor.
	/// </summary>
	public void Run()
	{
		Present();
	}

	/// <summary>
	/// Create and run a dialog allowing the user to select from a range of
	/// options which will be converted to strings using the given function.
	/// </summary>
	/// <param name="options">User choices/options.</param>
	/// <param name="nameSelector">Function which gets a string for an option.</param>
	/// <param name="prompt">Prompt text displayed to the user.</param>
	/// <param name="acceptText">Text on the 'accept' button.</param>
	/// <param name="itemSelected">Function to be called when an item is selected.</param>
	public static void RunFor<T>(IEnumerable<T> options, Func<T, string> nameSelector, string prompt, string acceptText,
		Action<T> itemSelected)
	{
		IEnumerable<string> names = options.Select(nameSelector);
		AskUserDialog dialog = new AskUserDialog(prompt, acceptText, names, resp =>
		{
			itemSelected(options.First(o => string.Equals(nameSelector(o), resp)));
		});
	}

	/// <summary>
	/// Connect all event sources to sinks.
	/// </summary>
	private void ConnectEvents()
	{
		foreach (ActionRow row in actionRows)
			row.OnActivated += OnSelected;
		cancelButton.OnClicked += OnCancel;
	}

	/// <summary>
	/// Disconnect all event sources from sinks.
	/// </summary>
	private void DisconnectEvents()
	{
		foreach (ActionRow row in actionRows)
			row.OnActivated -= OnSelected;
		cancelButton.OnClicked -= OnCancel;
	}

	/// <summary>
	/// Called when an item has been selected by the user.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnSelected(ActionRow sender, EventArgs args)
	{
		try
		{
			// This should never be null, as we assigned a title to every row.
			string? selection = sender.Title;
			if (selection != null)
				onSelected(selection);
			Hide();
			Dispose();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	private void OnCancel(Button sender, EventArgs args)
	{
		try
		{
			Hide();
			Dispose();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}
}
