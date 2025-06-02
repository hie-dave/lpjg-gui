using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Views;

using FileChooserDialog = LpjGuess.Frontend.Views.Dialogs.FileChooserDialog;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays the instruction files in a sidebar.
/// </summary>
public class InstructionFilesView : DynamicStackSidebar<string>, IInstructionFilesView
{
	/// <summary>
	/// The views used to display instruction files.
	/// </summary>
	private readonly List<IInstructionFileView> instructionFileViews;

	/// <summary>
	/// The name of the previously visible instruction file.
	/// </summary>
	private string? previouslyVisibleInsFile = null;

	/// <inheritdoc />
	public Event<string> OnAddInsFile { get; private init; }

	/// <inheritdoc />
	public Event<string> OnRemoveInsFile { get; private init; }

    /// <summary>
    /// Create a new <see cref="InstructionFilesView"/> instance.
    /// </summary>
    public InstructionFilesView() : base(CreateInsFileSidebarLabel)
    {
        OnAddInsFile = new Event<string>();
        OnRemoveInsFile = new Event<string>();
		instructionFileViews = new List<IInstructionFileView>();

        AddText = "Add File";
        OnPageSelected.ConnectTo(OnInsFilesSidebarPageSelected);
        OnRemove.ConnectTo(OnRemoveInsFile);
        OnAdd.ConnectTo(OnAddFile);
    }

    /// <summary>
    /// Populate the view with the given instruction files.
    /// </summary>
    /// <param name="insFileViews">The instruction files with which the view should be populated.</param>
    public void Populate(IEnumerable<IInstructionFileView> insFileViews)
	{
        instructionFileViews.Clear();
        instructionFileViews.AddRange(insFileViews);

		// Populate the stack with new views.
        Populate(insFileViews.Select(f => (GetTabName(f.Name), f.GetWidget())));
	}

    /// <inheritdoc />
    public Widget GetWidget() => this;

    /// <summary>
    /// Get an appropriate name for a tab containing an instruction file.
    /// </summary>
    /// <param name="insFile">Path to an instruction file.</param>
    /// <returns>A tab name.</returns>
    private static string GetTabName(string insFile)
    {
        return Path.GetFileName(insFile);
    }

    /// <summary>
    /// Get the name of a tab in the sidebar corresponding to a particular
    /// instruction file.
    /// </summary>
    /// <param name="instructionFile">The instruction file.</param>
    /// <returns>A label for the sidebar tab.</returns>
    private static Widget CreateInsFileSidebarLabel(string instructionFile)
    {
        Label label = Label.New(GetTabName(instructionFile));
        label.Halign = Align.Start;
        label.Hexpand = true;
        return label;
    }

    /// <summary>
    /// Called when the user wants to add an instruction file.
    /// </summary>
    private void OnAddFile()
    {
		FileChooserDialog fileChooser = FileChooserDialog.Open(
			"Open Instruction File",
			"Instruction Files",
			"*.ins",
			true,
			false);
		fileChooser.OnFileSelected.ConnectTo(OnAddInsFile);
		fileChooser.Run();
		return;
    }

	/// <summary>
	/// User wants to add an instruction file.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnAddInstructionFile(object sender, EventArgs args)
	{
		try
		{
			FileChooserDialog dialog = FileChooserDialog.Open(
				"Open Instruction File",
				"Instruction Files",
				"*.ins",
				true,
				false);
			dialog.OnFileSelected.ConnectTo(OnAddInsFile);
			dialog.Run();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Callback for the instruction files' stack's "notify" signal.
	/// </summary>
	/// <param name="page">The page that was selected.</param>
	private void OnInsFilesSidebarPageSelected(string page)
	{
		try
		{
			// The visible child of the stack has changed.
			if (page == "Add File")
			{
				// The user has clicked the "Add File" button. This button
				// is an entry in the sidebar with a corresponding blank
				// widget in the stack. Therefore we try to reset the
				// visible child to the previously-selected ins file.
				if (previouslyVisibleInsFile == null)
				{
					// No ins file was previously selected. Try to select
					// the last ins file (ie the one closest to the button).
					if (instructionFileViews.Count > 0)
						VisibleChildName = GetTabName(instructionFileViews.Last().Name);
					// else user has clicked Add File in a workspace with no
					// instruction files - nothing we can do.
				}
				else
					VisibleChildName = previouslyVisibleInsFile;

				// Handle the "Add File" action by prompting the user to
				// select an instruction file.
				OnAddInstructionFile(this, EventArgs.Empty);
			}
			else
				previouslyVisibleInsFile = VisibleChildName;
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
    }
}
