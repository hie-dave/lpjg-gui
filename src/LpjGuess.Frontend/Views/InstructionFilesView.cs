using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Views;

using FileChooserDialog = LpjGuess.Frontend.Views.Dialogs.FileChooserDialog;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays the instruction files in a sidebar.
/// </summary>
public class InstructionFilesView : DynamicStackSidebar<IInstructionFileView>, IInstructionFilesView
{
	/// <summary>
	/// The views used to display instruction files.
	/// </summary>
	private readonly List<IInstructionFileView> instructionFileViews;

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
        OnRemove.ConnectTo(OnRemoveView);
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
        Populate(insFileViews.Select(f => (f, f.GetWidget())));
	}

    /// <summary>
    /// Get an appropriate name for a tab containing an instruction file.
    /// </summary>
    /// <param name="insFile">Path to an instruction file.</param>
    /// <returns>A tab name.</returns>
    private static string GetTabName(IInstructionFileView insFile)
    {
        return Path.GetFileName(insFile.Name);
    }

    /// <summary>
    /// Get the name of a tab in the sidebar corresponding to a particular
    /// instruction file.
    /// </summary>
    /// <param name="instructionFile">The instruction file.</param>
    /// <returns>A label for the sidebar tab.</returns>
    private static Widget CreateInsFileSidebarLabel(IInstructionFileView instructionFile)
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
	/// Called when the user wants to remove an instruction file.
	/// </summary>
    private void OnRemoveView(IInstructionFileView view)
    {
        OnRemoveInsFile.Invoke(view.Name);
    }
}
