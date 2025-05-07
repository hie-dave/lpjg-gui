using Gtk;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays an instruction file to the user.
/// </summary>
public class InstructionFileView : ScrolledWindow, IInstructionFileView
{
    /// <summary>
    /// The view which displays the contents of the file.
    /// </summary>
    private readonly EditorView editor;

    /// <summary>
    /// The file being displayed by this view.
    /// </summary>
    public string File { get; private init; }

    /// <summary>
    /// Create a new <see cref="InstructionFileView"/> instance for the
    /// specified file.
    /// </summary>
    /// <param name="file">The instruction file to display.</param>
    public InstructionFileView(string file)
    {
        File = file;
        editor = new EditorView();
        editor.AppendLine(System.IO.File.ReadAllText(file));
        Child = editor.GetWidget();
    }

    /// <inheritdoc />
    public Widget GetWidget() => this;
}
