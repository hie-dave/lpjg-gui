using LpjGuess.Core.Extensions;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views;
using LpjGuess.Runner;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for an instruction file view.
/// </summary>
public class InstructionFilePresenter : PresenterBase<IInstructionFileView>, IInstructionFilePresenter
{
    /// <summary>
    /// The instruction file being displayed.
    /// </summary>
    private readonly string insFile;

    /// <summary>
    /// The cancellation token.
    /// </summary>
    private readonly CancellationTokenSource cts;

    /// <summary>
    /// The editor views.
    /// </summary>
    private readonly List<IEditorView> editors;

    /// <summary>
    /// Create a new <see cref="InstructionFilePresenter"/> instance for the
    /// specified file.
    /// </summary>
    /// <param name="insFile">The instruction file to display.</param>
    public InstructionFilePresenter(string insFile) : this(insFile, new InstructionFileView(Path.GetFileName(insFile)))
    {
    }

    /// <summary>
    /// Create a new <see cref="InstructionFilePresenter"/> instance for the
    /// specified view.
    /// </summary>
    /// <param name="view">The view to be controlled by this presenter.</param>
    /// <param name="insFile">The instruction file to display.</param>
    public InstructionFilePresenter(string insFile, IInstructionFileView view) : base(view)
    {
        this.insFile = insFile;
        editors = new List<IEditorView>();

        // Fire and forget. The CancellationTokenSource will be disposed of
        // when the presenter is disposed.
        cts = new CancellationTokenSource();
        Task _ = PopulateViewAsync(cts.Token);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        base.Dispose();
        cts.Cancel();
        cts.Dispose();
    }

    /// <summary>
    /// Populate the view with the contents of the instruction file.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    private async Task PopulateViewAsync(CancellationToken ct)
    {
        view.Clear();
        editors.ForEach(e => e.OnChanged.DisconnectAll());
        editors.Clear();

        await AddViewAsync(insFile, ct);
        IEnumerable<string> imports = InstructionFileNormaliser.ResolveImportDirectives(insFile);
        foreach (string imported in imports)
            await AddViewAsync(imported, ct);
    }

    /// <summary>
    /// Add a view for a child instruction file.
    /// </summary>
    /// <param name="file">The file to add.</param>
    /// <param name="ct">The cancellation token.</param>
    private async Task AddViewAsync(string file, CancellationToken ct)
    {
        string text = await File.ReadAllTextAsync(file, ct);
        EditorView child = new EditorView();
        child.AppendLine(text);
        view.AddView(Path.GetFileName(file), child);
        child.OnChanged.ConnectTo(() => OnEditorChanged(child));
        editors.Add(child);
    }

    /// <summary>
    /// Called when the contents of an editor view change.
    /// </summary>
    /// <param name="editor">The editor view which has changed.</param>
    private void OnEditorChanged(IEditorView editor)
    {
        try
        {
            // TODO: Implement this.
            view.FlagChanged(editor);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
