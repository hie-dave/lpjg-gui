using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for an experiment view.
/// </summary>
public class ExperimentPresenter : PresenterBase<IExperimentView>, IExperimentPresenter
{
    /// <summary>
    /// The experiment being presented.
    /// </summary>
    private readonly Experiment experiment;

    /// <summary>
    /// Create a new <see cref="ExperimentPresenter"/> instance.
    /// </summary>
    /// <param name="experiment">The experiment to present.</param>
    /// <param name="view">The view to present the experiment on.</param>
    public ExperimentPresenter(Experiment experiment, IExperimentView view) : base(view)
    {
        this.experiment = experiment;
        view.OnChanged.ConnectTo(OnChanged);
        RefreshView();
    }

    /// <inheritdoc />
    public Experiment GetExperiment() => experiment;

    /// <inheritdoc />
    public void UpdateInstructionFiles(IEnumerable<string> instructionFiles)
    {
        view.UpdateInstructionFiles(instructionFiles);
    }

    /// <summary>
    /// Called when the user wants to change the experiment.
    /// </summary>
    private void OnChanged(IModelChange<Experiment> change)
    {
        ICommand command = change.ToCommand(experiment);
        command.Execute();
        RefreshView();
    }

    /// <summary>
    /// Refresh the view.
    /// </summary>
    private void RefreshView()
    {
        view.Populate(
            experiment.Name,
            experiment.Description,
            experiment.Runner,
            experiment.InstructionFiles,
            experiment.Pfts,
            experiment.Factorials);
    }
}
