using Adw;
using Gtk;
using Window = Adw.Window;
using HeaderBar = Adw.HeaderBar;
using LpjGuess.Frontend.Utility.Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Classes;

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
	/// Action rows.
	/// </summary>
	private readonly IReadOnlyList<ActionRow> actionRows;

	/// <summary>
	/// Function to be called when the select button is clicked. The selected
	/// option will be returned.
	/// </summary>
	public Event<string> OnSelected { get; private init; }

	/// <summary>
	/// Create a new <see cref="AskUserDialog"/> instance.
	/// </summary>
	/// <param name="prompt">Prompt to the user (displayed as window title).</param>
	/// <param name="acceptButtonText">Text to go on the 'accept' button.</param>
	/// <param name="options">Valid options.</param>
	public AskUserDialog(string prompt, string acceptButtonText, IEnumerable<NameAndDescription> options)
	{
		OnSelected = new Event<string>();
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
		foreach (NameAndDescription option in options)
		{
			ActionRow row = new ActionRow();
			row.Activatable = true;
			row.Title = option.Name;
			if (!string.IsNullOrWhiteSpace(option.Description))
				row.Subtitle = option.Description;
			choicesBox.Append(row);
			rows.Add(row);
		}
		actionRows = rows;

		ScrolledWindow choices = new ScrolledWindow();
		choices.MarginBottom = margin;
		choices.MarginTop = margin;
		choices.MarginStart = margin;
		choices.MarginEnd = margin;
		choices.Vexpand = true;
		choices.Child = choicesBox;

		Box main = new Box();
		main.SetOrientation(Orientation.Vertical);
		main.Append(header);
		main.Append(choices);

		Content = main;

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
	/// <param name="descriptionSelector">Function which gets a description for an option.</param>
	/// <param name="prompt">Prompt text displayed to the user.</param>
	/// <param name="acceptText">Text on the 'accept' button.</param>
	/// <param name="itemSelected">Function to be called when an item is selected.</param>
	public static void RunFor<T>(
		IEnumerable<T> options,
		Func<T, string> nameSelector,
		Func<T, string> descriptionSelector,
		string prompt,
		string acceptText,
		Action<T> itemSelected)
	{
		IEnumerable<NameAndDescription> names = options.Select(o => new NameAndDescription(nameSelector(o), descriptionSelector(o)));
		AskUserDialog dialog = new AskUserDialog(prompt, acceptText, names);
		dialog.OnSelected.ConnectTo(resp =>
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
			row.OnActivated += OnItemSelected;
		cancelButton.OnClicked += OnCancel;
	}

	/// <summary>
	/// Disconnect all event sources from sinks.
	/// </summary>
	private void DisconnectEvents()
	{
		foreach (ActionRow row in actionRows)
			row.OnActivated -= OnItemSelected;
		cancelButton.OnClicked -= OnCancel;
		OnSelected.DisconnectAll();
	}

	/// <summary>
	/// Called when an item has been selected by the user.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnItemSelected(ActionRow sender, EventArgs args)
	{
		try
		{
			// This should never be null, as we assigned a title to every row.
			string? selection = sender.Title;
			if (selection != null)
				OnSelected.Invoke(selection);
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
