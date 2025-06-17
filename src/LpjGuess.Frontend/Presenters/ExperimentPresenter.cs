using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Frontend.Delegates;
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
    /// The factorial presenter.
    /// </summary>
    private readonly IFactorialPresenter factorialPresenter;

    /// <summary>
    /// The available instruction files in the workspace.
    /// </summary>
    private IEnumerable<string> instructionFiles;

    /// <inheritdoc />
    public Event<string> OnRenamed { get; private init; }

    /// <summary>
    /// Create a new <see cref="ExperimentPresenter"/> instance.
    /// </summary>
    /// <param name="instructionFiles">The available instruction files in the workspace.</param>
    /// <param name="experiment">The experiment to present.</param>
    /// <param name="view">The view to present the experiment on.</param>
    public ExperimentPresenter(
        IEnumerable<string> instructionFiles,
        Experiment experiment,
        IExperimentView view) : base(view)
    {
        OnRenamed = new Event<string>();
        this.instructionFiles = instructionFiles;
        this.experiment = experiment;
        view.OnChanged.ConnectTo(OnExperimentChanged);
        factorialPresenter = new FactorialPresenter(view.FactorialView);
        factorialPresenter.OnChanged.ConnectTo(OnSimulationGeneratorChanged);
        RefreshView();
    }

    /// <inheritdoc />
    public Experiment GetExperiment() => experiment;

    /// <inheritdoc />
    public void UpdateInstructionFiles(IEnumerable<string> instructionFiles)
    {
        this.instructionFiles = instructionFiles;
        view.UpdateInstructionFiles(instructionFiles.Select(f => (f, experiment.InstructionFiles.Contains(f))));
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        view.OnChanged.DisconnectFrom(OnExperimentChanged);
        factorialPresenter.Dispose();
        base.Dispose();
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
            instructionFiles.Select(f => (f, experiment.InstructionFiles.Contains(f))),
            experiment.Pfts);

        factorialPresenter.Populate(GetFactorialGenerator());
    }

    /// <summary>
    /// Get the factorial generator from the experiment.
    /// </summary>
    /// <returns>The factorial generator.</returns>
    /// <remarks>
    /// This is horrible. Need to rethink if we actually want such a high-level
    /// interface for the generator property, and whether there will actually be
    /// other implementations that we need to support going forward.
    /// </remarks>
    private FactorialGenerator GetFactorialGenerator()
    {
        if (experiment.SimulationGenerator is not FactorialGenerator generator)
            throw new NotImplementedException($"{experiment.SimulationGenerator.GetType().Name} is not yet supported.");
        return generator;
    }

    /// <summary>
    /// Invoke the specified command.
    /// </summary>
    /// <param name="command">The command to invoke.</param>
    private void InvokeCommand(ICommand command)
    {
        // This will become slightly less trivial once we have a command
        // history.
        string oldName = experiment.Name;
        command.Execute();
        if (experiment.Name != oldName)
            OnRenamed.Invoke(experiment.Name);
    }

    /// <summary>
    /// Called when the user wants to change the experiment.
    /// </summary>
    /// <param name="change">The change to apply.</param>
    private void OnExperimentChanged(IModelChange<Experiment> change)
    {
        ICommand command = change.ToCommand(experiment);
        InvokeCommand(command);
        RefreshView();
    }

    /// <summary>
    /// Called when the simulation generator changes.
    /// </summary>
    /// <param name="change">The change to apply.</param>
    private void OnSimulationGeneratorChanged(IModelChange<FactorialGenerator> change)
    {
        ICommand command = change.ToCommand(GetFactorialGenerator());
        InvokeCommand(command);
    }
}
