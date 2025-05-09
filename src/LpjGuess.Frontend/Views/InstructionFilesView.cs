using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Utility.Gtk;

using FileChooserDialog = LpjGuess.Frontend.Views.Dialogs.FileChooserDialog;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A class which displays a list of instruction files to the user.
/// </summary>
public class InstructionFilesView : DynamicStackSidebar
{
    /// <summary>
    /// Spacing, in pixels, between elements in the title box in the sidebar.
    /// </summary>
    private const int spacing = 6;

    /// <summary>
    /// Name of the widget associated with the 'add file' menu option.
    /// </summary>
    private const string addFileName = "add-file";

    /// <summary>
    /// The name of the previously selected instruction file.
    /// </summary>
    private string? previouslySelectedFile = null;

    /// <summary>
    /// The names of the instruction files currently displayed.
    /// </summary>
    private readonly List<string> pageNames;

    /// <summary>
    /// Event which is raised when the user wants to add an instruction file.
    /// </summary>
    public Event<string> OnAdd { get; private init; }

    /// <summary>
    /// Event which is raised when the user wants to remove an instruction file.
    /// </summary>
    public Event<string> OnRemove { get; private init; }

    /// <summary>
    /// Create a new <see cref="InstructionFilesView"/> instance.
    /// </summary>
    public InstructionFilesView() : base()
    {
        OnAdd = new Event<string>();
        OnRemove = new Event<string>();
        pageNames = new List<string>();
    }

    /// <summary>
    /// Populate the view with the given pages, as well as an "Add File" entry.
    /// </summary>
    /// <param name="pages">The pages to be displayed.</param>
    public override void Populate(IEnumerable<(string, Widget)> pages)
    {
        pageNames.Clear();
        pageNames.AddRange(pages.Select(p => p.Item1));

        base.Populate(pages);

        // Add an "Add File" entry.
        Label label = Label.New("Add File");
        label.Halign = Align.Start;
        label.Hexpand = true;
        AddEntry(addFileName, new Box(), label);
    }

    /// <summary>
    /// Callback for the sidebar's "row-activated" signal. Handle the "Add File"
    /// option, if that's what was clicked. Otherwise, the event will propagate
    /// up to the base class.
    /// </summary>
    /// <param name="sender">The sidebar.</param>
    /// <param name="args">The row-activated signal arguments.</param>
    protected override void OnSidebarRowActivated(ListBox sender, ListBox.RowActivatedSignalArgs args)
    {
        try
        {
			string? name = args.Row.Name;
            if (name == null)
                return;

            if (name == addFileName)
            {
                if (previouslySelectedFile != null)
                    VisibleChildName = previouslySelectedFile;
                else if (pageNames.Any())
                    VisibleChildName = pageNames.Last();

                FileChooserDialog fileChooser = FileChooserDialog.Open(
                    "Open Instruction File",
                    "Instruction Files",
                    "*.ins",
                    true,
                    false);
                fileChooser.OnFileSelected.ConnectTo(OnAdd);
                fileChooser.Run();
                return;
            }
            previouslySelectedFile = VisibleChildName;
            Console.WriteLine($"Visible child: {VisibleChildName}");
            base.OnSidebarRowActivated(sender, args);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Create a widget for the stack element with the specified title which
    /// will be displayed in the sidebar. This implementation creates a label
    /// with a menu button with a delete action.
    /// </summary>
    /// <param name="path">Path to the instruction file.</param>
    /// <returns>A widget to be displayed in the sidebar.</returns>
    protected override Widget CreateWidget(string path)
    {
        string fileName = Path.GetFileName(path);

        Box box = Box.New(Orientation.Horizontal, spacing);
        Label label = Label.New(fileName);
        label.Hexpand = true;
        label.Halign = Align.Start;
        box.Append(label);

        Button menuButton = new Button();
        menuButton.IconName = Icons.Delete;
        menuButton.AddCssClass("flat");
        menuButton.Halign = Align.End;
        menuButton.OnClicked += (_, __) => RemoveFile(path);

        box.Append(menuButton);

        return box;
    }

    /// <summary>
    /// Called when the user wants to remove an instruction file.
    /// </summary>
    /// <param name="name">Name of the instruction file.</param>
    private void RemoveFile(string name)
    {
        try
        {
            OnRemove.Invoke(name);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
