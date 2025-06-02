using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
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
        presenters = new List<IInstructionFilePresenter>();
        foreach (string file in insFiles)
            presenters.Add(new InstructionFilePresenter(file));
        view.Populate(presenters.Select(p => p.GetView()));
    }
}
