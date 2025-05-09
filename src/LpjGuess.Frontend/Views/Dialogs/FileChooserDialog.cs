using Gio;
using Gtk;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Views.Dialogs;

/// <summary>
/// A helper class for opening file chooser dialogs.
/// </summary>
public class FileChooserDialog
{
    /// <summary>
    /// The file chooser dialog.
    /// </summary>
    private readonly FileChooserNative fileChooser;

    /// <summary>
    /// Invoked when the user selects a file. This is only invoked if selection
    /// of multiple files is disabled.
    /// </summary>
    public Event<string> OnFileSelected { get; private init; }

    /// <summary>
    /// Invoked when the user selects a file or files. This is only invoked if
    /// selection of multiple files is enabled. In that case, this will be used
    /// instead of <see cref="OnFileSelected"/>, even if the user selects a
    /// single file.
    /// </summary>
    public Event<IEnumerable<string>> OnFilesSelected { get; private init; }

    /// <summary>
    /// Opens a file chooser dialog.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="filterName">The name of the filter to apply.</param>
    /// <param name="filterPattern">The pattern to apply to the filter.</param>
    /// <param name="allowAllFiles">Whether to allow all files to be selected.</param>
    /// <param name="allowMultiple">Whether to allow multiple files to be selected.</param>
    /// <param name="action">The action to perform when the user selects a file.</param>
    private FileChooserDialog(string title,
		string filterName,
		string filterPattern,
		bool allowAllFiles,
        bool allowMultiple,
        FileChooserAction action)
    {
        OnFileSelected = new Event<string>();
        OnFilesSelected = new Event<IEnumerable<string>>();

        string acceptText = GetAcceptText(action);

        fileChooser = FileChooserNative.New(
			title,
			MainView.Instance,
			action,
			acceptText,
			"Cancel"
		);

        // Block main window while dialog is open.
		fileChooser.SetModal(true);
        fileChooser.TransientFor = MainView.Instance;
        if (Configuration.Instance.PreviousDirectory != null)
        {
            Gio.File path = Gio.Functions.FileNewForPath(Configuration.Instance.PreviousDirectory); 
            fileChooser.SetCurrentFolder(path);
        }

        // Add file filters.
		FileFilter filter = FileFilter.New();
		filter.AddPattern(filterPattern);
		filter.Name = filterName;
		fileChooser.AddFilter(filter);
        fileChooser.SelectMultiple = allowMultiple;

		if (allowAllFiles)
		{
			FileFilter filterAll = FileFilter.New();
			filterAll.AddPattern("*");
			filterAll.Name = "All Files";
			fileChooser.AddFilter(filterAll);
		}

		fileChooser.OnResponse += OnResponse;
    }

    /// <summary>
    /// Run the dialog. Non-blocking. The result is obtained via the events
    /// <see cref="OnFileSelected"/> and <see cref="OnFilesSelected"/> .
    /// </summary>
    public void Run()
    {
		fileChooser.Show();
    }

    private string GetAcceptText(FileChooserAction action)
    {
        if (action == FileChooserAction.Open)
            return "Open";

        if (action == FileChooserAction.Save)
            return "Save";

        if (action == FileChooserAction.SelectFolder)
            return "Select Folder";

        throw new ArgumentException($"Invalid action: {action}");
    }

    /// <summary>
    /// Create a new <see cref="FileChooserDialog"/> instance which allows the
    /// user to select one (or more, if <paramref name="allowMultiple"/> is
    /// true) existing files.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="filterName">Name of the file filter.</param>
    /// <param name="filterPattern">File pattern to match.</param>
    /// <param name="allowAllFiles">True to allow the user to select any file.</param>
    /// <param name="allowMultiple">True to allow multiple files to be selected.</param>
    /// <returns>A <see cref="FileChooserDialog"/> instance.</returns>
    public static FileChooserDialog Open(
        string title,
		string filterName,
		string filterPattern,
		bool allowAllFiles,
        bool allowMultiple
    )
    {
        return new FileChooserDialog(
            title,
            filterName,
            filterPattern,
            allowAllFiles,
            allowMultiple,
            FileChooserAction.Open
        );
    }

    /// <summary>
    /// Create a new <see cref="FileChooserDialog"/> instance which allows the
    /// user to select a file path which does not already exist.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="filterName">Name of the file filter.</param>
    /// <param name="filterPattern">File pattern to match.</param>
    /// <param name="allowAllFiles">True to allow the user to select any file.</param>
    /// <returns>A <see cref="FileChooserDialog"/> instance.</returns>
    public static FileChooserDialog Save(
        string title,
		string filterName,
		string filterPattern,
		bool allowAllFiles
    )
    {
        return new FileChooserDialog(
            title,
            filterName,
            filterPattern,
            allowAllFiles,
            false,
            FileChooserAction.Save
        );
    }

    /// <summary>
    /// Called when the user has responded to the file chooser dialog.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnResponse(NativeDialog sender, NativeDialog.ResponseSignalArgs args)
    {
        try
        {
            if (sender is FileChooserNative fileChooser &&
                args.ResponseId == (int)ResponseType.Accept)
            {
                fileChooser.OnResponse -= OnResponse;

                if (fileChooser.SelectMultiple)
                {
                    ListModel model = fileChooser.GetFiles();
                    for (uint i = 0; i < model.GetNItems(); i++)
                    {
                        nint item = model.GetItem(i);
                    }
                    // OnFilesSelected.Invoke(selectedFiles);
                    // When we implement this, remember to update Configuration.Instance.PreviousDirectory.
                    throw new NotImplementedException("TBI: selection of multiple files.");
                }
                else
                {
                    string? selectedFile = fileChooser.GetFile()?.GetPath();
                    if (!string.IsNullOrEmpty(selectedFile))
                    {
                        Configuration.Instance.PreviousDirectory = Path.GetDirectoryName(selectedFile);
                        Configuration.Instance.Save();
                        OnFileSelected.Invoke(selectedFile);
                    }
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
