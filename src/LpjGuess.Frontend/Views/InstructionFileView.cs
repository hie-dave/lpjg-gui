using Gtk;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays an instruction file to the user.
/// </summary>
/// <remarks>
/// TODO: gtksourceview. But this requires bindings...
/// TODO: Handle multiple files.
/// </remarks>
public class InstructionFileView : ViewBase<Notebook>, IInstructionFileView
{
    /// <summary>
    /// The view which displays the contents of the file.
    /// </summary>
    private readonly List<Tab> editors;

    /// <inheritdoc />
    public string Name { get; private init; }

    /// <summary>
    /// Create a new <see cref="InstructionFileView"/> instance for the
    /// specified file.
    /// </summary>
    public InstructionFileView(string name) : base(new Notebook())
    {
        Name = name;
        editors = new List<Tab>();
    }

    /// <inheritdoc />
    public void AddView(string name, IEditorView editor)
    {
        Label label = Label.New(name);
        Widget child = editor.GetWidget();
        widget.AppendPage(child, label);
        editors.Add(new Tab(name, child, label));
    }

    /// <inheritdoc />
    public void Clear()
    {
        editors.Clear();
        while (widget.GetNPages() > 0)
            widget.RemovePage(0);
    }

    /// <inheritdoc />
    public void FlagChanged(IEditorView editor)
    {
        Tab tab = GetTab(editor);
        tab.Label.SetMarkup($"<b>{tab.Name}</b>");
    }

    /// <summary>
    /// Get the tab corresponding to the specified editor.
    /// </summary>
    /// <param name="editor">The editor.</param>
    /// <returns>The tab.</returns>
    private Tab GetTab(IEditorView editor)
    {
        Tab? tab = editors.FirstOrDefault(t => t.Widget == editor.GetWidget());
        if (tab is null)
            throw new ArgumentException($"Editor widget not found");
        return tab;
    }

    /// <inheritdoc />
    public void UnflagChanges()
    {
        foreach (Tab tab in editors)
            tab.Label.SetMarkup(tab.Name);
    }

    /// <summary>
    /// A tab in the notebook.
    /// </summary>
    private class Tab
    {
        public string Name { get; }
        public Widget Widget { get; }
        public Label Label { get; }
        public Tab(string name, Widget widget, Label label)
        {
            Name = name;
            Widget = widget;
            Label = label;
        }
    }
}
