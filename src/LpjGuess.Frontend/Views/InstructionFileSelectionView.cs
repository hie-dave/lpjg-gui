using Gtk;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which allows the user to select an instruction file.
/// </summary>
public class InstructionFileSelectionView : CustomColumnView<string>
{
    /// <summary>
    /// The selected instruction files.
    /// </summary>
    private HashSet<string> selectedInstructionFiles;

    /// <summary>
    /// All instruction files.
    /// </summary>
    private List<string> allInstructionFiles;

    /// <summary>
    /// Called when the selected instruction files are changed by the user.
    /// </summary>
    public Event<IEnumerable<string>> OnSelectionChanged { get; private init; }

    /// <summary>
    /// Get the current selection.
    /// </summary>
    public IEnumerable<(string, bool)> GetSelection()
    {
        return allInstructionFiles.Select(f => (f, selectedInstructionFiles.Contains(f)));
    }

    /// <summary>
    /// Create a new <see cref="InstructionFileSelectionView"/> instance.
    /// </summary>
    public InstructionFileSelectionView() : base()
    {
        OnSelectionChanged = new Event<IEnumerable<string>>();
        selectedInstructionFiles = [];
        allInstructionFiles = [];

        AddColumn("Selected", CreateCheckButton, BindCheckButton);
        AddColumn("File", CreateLabel, BindLabel);
    }

    /// <summary>
    /// Populate this view with the specified instruction files.
    /// </summary>
    /// <param name="allFiles">All instruction files.</param>
    /// <param name="selectedFiles">The selected instruction files.</param>
    public void Populate(IEnumerable<string> allFiles, IEnumerable<string> selectedFiles)
    {
        RemoveRows();
        selectedInstructionFiles = selectedFiles.ToHashSet();
        allInstructionFiles = allFiles.ToList();
        Populate(allInstructionFiles);
    }

    /// <summary>
    /// Bind a label to a row.
    /// </summary>
    /// <param name="text">The text to display in the label.</param>
    /// <param name="label">The label to bind.</param>
    private void BindLabel(string text, Label label)
    {
        label.SetText(Path.GetFileName(text));
    }

    /// <summary>
    /// Bind a check button to a row.
    /// </summary>
    /// <param name="instructionFile">The instruction file.</param>
    /// <param name="button">The check button to bind.</param>
    private void BindCheckButton(string instructionFile, CheckButton button)
    {
        button.SetActive(selectedInstructionFiles.Contains(instructionFile));
        button.Name = instructionFile;
        button.OnToggled += OnCheckButtonToggled;
    }

    /// <summary>
    /// Create a check button.
    /// </summary>
    /// <returns>A check button.</returns>
    private CheckButton CreateCheckButton()
    {
        return new CheckButton();
    }

    /// <summary>
    /// Create a label.
    /// </summary>
    /// <returns>A label.</returns>
    private Label CreateLabel()
    {
        return new Label() { Halign = Align.Start };
    }

    /// <summary>
    /// Called when a check button is toggled by the user.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnCheckButtonToggled(CheckButton sender, EventArgs args)
    {
        try
        {
            string insFile = sender.Name!;
            if (!sender.Active)
                selectedInstructionFiles.Remove(insFile);
            else
                selectedInstructionFiles.Add(insFile);
            OnSelectionChanged.Invoke(selectedInstructionFiles);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
