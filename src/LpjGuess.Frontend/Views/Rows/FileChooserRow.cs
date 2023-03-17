using Adw;
using Gtk;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Views.Rows;

/// <summary>
/// A preferences row which allows the user to input a file name by typing or by
/// using a file chooser widget.
/// </summary>
internal class FileChooserRow : EntryRow
{
	/// <summary>
	/// File extensions allowed to be selected by the user.
	/// </summary>
	private readonly IEnumerable<FileFilter> filters;

	private readonly Button fileChooserButton;

	/// <summary>
	/// Called when the user has changed the file path.
	/// </summary>
	public Event<string> OnChanged { get; private init; }

	/// <summary>
	/// Create a new <see cref="FileChooserRow"/> instance.
	/// </summary>
	/// <param name="title">Row title text.</param>
	/// <param name="file">The initial file path.</param>
	/// <param name="allowAnyFile">True to allow the user to select any file in the dialog. False to limit it to only those specified by the extensions argument.</param>
	/// <param name="extensions">Array of tuples of (name, extension), where name is a description of the extension. E.g. ("Text Files", "*.txt").</param>
	public FileChooserRow(string title, string file, bool allowAnyFile, params (string, string)[] extensions)
	{
		Title = title;
		SetText(file);
		this.filters = GetFilters(allowAnyFile, extensions);
		OnChanged = new Event<string>();

		fileChooserButton = Button.NewWithLabel("...");
		fileChooserButton.Valign = Align.Center;
		AddSuffix(fileChooserButton);
		ConnectEvents();
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public override void Dispose()
	{
		DisconnectEvents();
		foreach (FileFilter filter in filters)
			filter.Dispose();
		base.Dispose();
	}

	/// <summary>
	/// Connect all events.
	/// </summary>
	private void ConnectEvents()
	{
		fileChooserButton.OnClicked += OnChooseFile;
		OnApply += OnFileChanged;
	}

	/// <summary>
	/// Disconnect all events.
	/// </summary>
	private void DisconnectEvents()
	{
		fileChooserButton.OnClicked -= OnChooseFile;
		OnApply -= OnFileChanged;
	}

	/// <summary>
	/// Parse a list of FileFilter objects from the specified file extensions.
	/// </summary>
	/// <param name="allowAnyFile">True to allow the user to select any file in the dialog. False to limit it to only those specified by the extensions argument.</param>
	/// <param name="extensions">Array of tuples of (name, extension), where name is a description of the extension. E.g. ("Text Files", "*.txt").</param>
	private IEnumerable<FileFilter> GetFilters(bool allowAnyFile, params (string, string)[] extensions)
	{
		List<FileFilter> filters = new List<FileFilter>();
		foreach ((string name, string extension) in extensions)
		{
			FileFilter filter = FileFilter.New();
			filter.Name = name;
			filter.AddPattern(extension);
			filters.Add(filter);
		}

		if (allowAnyFile)
		{
			FileFilter filterAll = FileFilter.New();
			filterAll.Name = "All Files";
			filterAll.AddPattern("*");
			filters.Add(filterAll);
		}
		return filters;
	}

	/// <summary>
	/// Called when the user applies changes to the file name.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnFileChanged(EntryRow sender, EventArgs args)
	{
		try
		{
			OnChanged.Invoke(GetText());
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Called when the user wants to use a file chooser to select the file.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnChooseFile(Button sender, EventArgs args)
	{
		try
		{
			FileChooserNative fileChooser = FileChooserNative.New(
				"Choose a file",
				MainView.Instance,
				FileChooserAction.Open,
				"Select",
				"Cancel");
			foreach (FileFilter filter in filters)
				fileChooser.AddFilter(filter);
			fileChooser.OnResponse += OnFileChosen;
			fileChooser.Show();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Called when the user has selected a file in the file chooser dialog.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnFileChosen(NativeDialog sender, NativeDialog.ResponseSignalArgs args)
	{
		try
		{
			sender.OnResponse -= OnFileChosen;
			if (sender is FileChooserNative fileChooser &&
				args.ResponseId == (int)ResponseType.Accept)
			{
				string? selectedFile = fileChooser.GetFile()?.GetPath();
				if (!string.IsNullOrEmpty(selectedFile))
				{
					SetText(selectedFile);
					OnChanged.Invoke(selectedFile);
				}
			}
			sender.Dispose();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}
}
