using LpjGuess.Core.Extensions;
using LpjGuess.Core.Parsers;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Factories;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for an instruction file view.
/// </summary>
[RegisterPresenter(typeof(string), typeof(IInstructionFilePresenter))]
public class InstructionFilePresenter : PresenterBase<IInstructionFileView, string>, IInstructionFilePresenter
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
    private readonly List<Editor> editors;

    /// <summary>
    /// The files with pending changes.
    /// </summary>
    private readonly HashSet<string> changedFiles;

    /// <summary>
    /// The view factory.
    /// </summary>
    private readonly IViewFactory viewFactory;

    /// <inheritdoc />
    public Event<FileChangedArgs> OnFileChanged { get; private init; }

    /// <inheritdoc />
    public Event<string> OnSaved { get; private init; }

    /// <summary>
    /// Create a new <see cref="InstructionFilePresenter"/> instance for the
    /// specified view.
    /// </summary>
    /// <param name="view">The view to be controlled by this presenter.</param>
    /// <param name="insFile">The instruction file to display.</param>
    /// <param name="commandRegistry">The command registry.</param>
    /// <param name="viewFactory">The view factory.</param>
    public InstructionFilePresenter(
        string insFile,
        IInstructionFileView view,
        ICommandRegistry commandRegistry,
        IViewFactory viewFactory) : base(view, insFile, commandRegistry)
    {
        this.insFile = insFile;
        this.viewFactory = viewFactory;
        OnFileChanged = new Event<FileChangedArgs>();
        OnSaved = new Event<string>();
        editors = new List<Editor>();
        changedFiles = new HashSet<string>();

        view.Name = insFile;

        // Fire and forget. The CancellationTokenSource will be disposed of
        // when the presenter is disposed.
        cts = new CancellationTokenSource();
        Task _ = PopulateViewAsync(cts.Token);
    }

    /// <inheritdoc />
    public void SaveChanges()
    {
        foreach (string file in changedFiles)
        {
            Editor editor = GetEditor(file);
            File.WriteAllText(file, editor.View.GetContents());
            view.UnflagChanges(editor.View);
            changedFiles.Remove(file);
            OnSaved.Invoke(file);
        }
    }

    /// <inheritdoc />
    public void NotifyFileSaved(string file)
    {
        if (changedFiles.Contains(file))
        {
            Editor editor = GetEditor(file);
            view.UnflagChanges(editor.View);
            changedFiles.Remove(file);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method is called when the contents of an editor view have been
    /// changed by another presenter, which typically happen when the user makes
    /// changes in an editor view managed by another presenter. If the file that
    /// was edited is *also* managed by this presenter (ie if multiple
    /// presenters manage the same file), then this method must apply those
    /// changes to the appropriate editor.
    /// </remarks>
    public void NotifyFileChanged(FileChangedArgs args)
    {
        // This is currently redundant.
        if (args.Presenter == this)
            return;

        Editor editor = GetEditor(args.File);
        editor.View.Populate(args.Contents);
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
        // If this is ever called from somewhere other than the constructor (ie
        // more than once per presenter lifetime), we will need to ensure that
        // the previously running task is correctly canceled before proceeding.
        editors.ForEach(e => e.View.OnChanged.DisconnectAll());
        view.Clear();
        editors.Clear();

        await AddViewAsync(insFile, ct);
        IEnumerable<string> imports = InstructionFileNormaliser.ResolveImportDirectives(insFile);
        foreach (string imported in imports.Distinct())
            await AddViewAsync(imported, ct);
    }

    /// <summary>
    /// Add a view for a child instruction file.
    /// </summary>
    /// <param name="file">The file to add.</param>
    /// <param name="ct">The cancellation token.</param>
    private async Task AddViewAsync(string file, CancellationToken ct)
    {
        // await File.ReadAllTextAsync(file, ct).ContinueWithOnMainThread(text => AddViewFromFileAsync(file, text));
        string text = await File.ReadAllTextAsync(file, ct).ContinueOnMainThread(ct);
        IEditorView child = viewFactory.CreateView<IEditorView>();
        child.Populate(text);
        view.AddView(Path.GetFileName(file), child);
        Editor editor = new Editor(file, child);
        child.OnChanged.ConnectTo(() => OnEditorChanged(editor));
        editors.Add(editor);
    }

    /// <summary>
    /// Add a view for a child instruction file.
    /// </summary>
    /// <param name="file">The file to add.</param>
    /// <param name="text">The text of the file.</param>
    private void AddViewFromFileAsync(string file, string text)
    {
        IEditorView child = viewFactory.CreateView<IEditorView>();
        child.Populate(text);
        view.AddView(Path.GetFileName(file), child);
        Editor editor = new Editor(file, child);
        child.OnChanged.ConnectTo(() => OnEditorChanged(editor));
        editors.Add(editor);
    }

    /// <summary>
    /// Called when the contents of an editor view have been changed by the
    /// user.
    /// </summary>
    /// <param name="editor">The editor view which has changed.</param>
    private void OnEditorChanged(Editor editor)
    {
        changedFiles.Add(editor.File);
        view.FlagChanged(editor.View);
        OnFileChanged.Invoke(new FileChangedArgs(
            editor.File,
            editor.View.GetContents(),
            this
        ));
    }

    /// <summary>
    /// Get the editor object corresponding to the specified file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>The editor object.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the file is not managed by this presenter.</exception>
    private Editor GetEditor(string file)
    {
        Editor? editor = editors.FirstOrDefault(e => e.File == file);
        if (editor is null)
            // Bad accounting - this would indicate a bug.
            throw new InvalidOperationException($"Could not find editor for file {file}");
        return editor;
    }

    private class Editor
    {
        public string File { get; }
        public IEditorView View { get; }

        public Editor(string file, IEditorView view)
        {
            File = file;
            View = view;
        }
    }
}
