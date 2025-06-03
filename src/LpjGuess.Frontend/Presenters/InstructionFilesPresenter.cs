using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for an instruction files view.
/// </summary>
public class InstructionFilesPresenter : PresenterBase<IInstructionFilesView>, IInstructionFilesPresenter
{
    /// <summary>
    /// The instruction file presenters.
    /// </summary>
    private List<IInstructionFilePresenter> presenters;

    /// <inheritdoc />
    public Event<string> OnAddInsFile => view.OnAddInsFile;

    /// <inheritdoc />
    public Event<string> OnRemoveInsFile => view.OnRemoveInsFile;

    /// <summary>
    /// Create a new <see cref="InstructionFilesPresenter"/> instance.
    /// </summary>
    /// <param name="view">The view to present.</param>
    public InstructionFilesPresenter(IInstructionFilesView view) : base(view)
    {
        presenters = new List<IInstructionFilePresenter>();
    }

    /// <inheritdoc />
    public void Populate(IEnumerable<string> insFiles)
    {
        // Remove existing presenters.
        presenters.ForEach(p => p.OnFileChanged.DisconnectAll());
        presenters.ForEach(p => p.OnSaved.DisconnectAll());
        presenters = new List<IInstructionFilePresenter>();

        foreach (string file in insFiles)
        {
            InstructionFilePresenter presenter = new(file);
            presenter.OnFileChanged.ConnectTo(OnFileChanged);
            presenter.OnSaved.ConnectTo(f => OnFileSaved(presenter, f));
            presenters.Add(presenter);
        }
        view.Populate(presenters.Select(p => p.GetView()));
    }

    /// <summary>
    /// Save any pending changes to the instruction files.
    /// </summary>
    public void SaveChanges()
    {
        presenters.ForEach(p => p.SaveChanges());
    }

    /// <summary>
    /// Called when a file has been saved.
    /// </summary>
    /// <param name="presenter">The presenter that saved the file.</param>
    /// <param name="file">The file that has been saved.</param>
    private void OnFileSaved(IInstructionFilePresenter presenter, string file)
    {
        presenters.Where(p => p != presenter)
                  .ToList()
                  .ForEach(p => p.NotifyFileSaved(file));
    }

    /// <summary>
    /// Called when the file has been changed by the user. Propagate the event
    /// to all other presenters.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    private void OnFileChanged(FileChangedArgs args)
    {
        presenters.Where(p => p != args.Presenter)
                  .ToList()
                  .ForEach(p => p.NotifyFileChanged(args));
    }
}
