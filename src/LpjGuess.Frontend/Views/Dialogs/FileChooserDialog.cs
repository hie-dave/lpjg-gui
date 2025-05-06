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
    public FileChooserDialog(string title,
		string filterName,
		string filterPattern,
		bool allowAllFiles,
        bool allowMultiple)
    {
        OnFileSelected = new Event<string>();
        OnFilesSelected = new Event<IEnumerable<string>>();

        FileChooserNative fileChooser = FileChooserNative.New(
			title,
			MainView.Instance,
			FileChooserAction.Open,
			"Open",
			"Cancel"
		);
		fileChooser.SetModal(true);
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

		fileChooser.Show();
    }

    private void OnResponse(NativeDialog sender, NativeDialog.ResponseSignalArgs args)
    {
        try
        {
            if (sender is FileChooserNative fileChooser &&
                args.ResponseId == (int)ResponseType.Accept)
            {
                if (fileChooser.SelectMultiple)
                {
                    ListModel model = fileChooser.GetFiles();
                    for (uint i = 0; i < model.GetNItems(); i++)
                    {
                        nint item = model.GetItem(i);
                    }
                    // OnFilesSelected.Invoke(selectedFiles);
                }
                else
                {
                    string? selectedFile = fileChooser.GetFile()?.GetPath();
                    if (!string.IsNullOrEmpty(selectedFile))
                        OnFileSelected.Invoke(selectedFile);
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
