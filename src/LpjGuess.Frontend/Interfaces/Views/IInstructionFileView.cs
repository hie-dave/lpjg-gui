namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Interface to a view which displays the contents of a single instruction
/// file. Each recursively imported instruction file is displayed in its own
/// area in the view.
/// </summary>
public interface IInstructionFileView : IView
{
    /// <summary>
    /// The name of the instruction file.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Remove all child views.
    /// </summary>
    void Clear();

    /// <summary>
    /// Add a view for a child instruction file.
    /// </summary>
    /// <param name="name">Name of the view.</param>
    /// <param name="editor">The text editor widget.</param>
    void AddView(string name, IEditorView editor);

    /// <summary>
    /// Add a visual indicator that the specified editor contains unsaved
    /// changes.
    /// </summary>
    /// <param name="editor">The editor view which has changed.</param>
    void FlagChanged(IEditorView editor);

    /// <summary>
    /// Remove visual indicators for all editors' unsaved changes.
    /// </summary>
    void UnflagChanges();
}
