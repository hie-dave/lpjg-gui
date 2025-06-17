using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views;
using LpjGuess.Runner.Models;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which manages the experiments.
/// </summary>
public class ExperimentsPresenter : PresenterBase<IExperimentsView>, IExperimentsPresenter
{
    private List<IExperimentPresenter> presenters = [];
    private List<string> instructionFiles = [];

    /// <summary>
    /// Create a new <see cref="ExperimentsPresenter"/> instance.
    /// </summary>
    /// <param name="view">The view to present.</param>
    public ExperimentsPresenter(IExperimentsView view) : base(view)
    {
        view.AddText = "Add Experiment";
        view.OnAdd.ConnectTo(OnAdd);
        view.OnRemove.ConnectTo(OnRemove);
    }

    /// <inheritdoc />
    public IEnumerable<Experiment> GetExperiments()
    {
        return presenters.Select(p => p.GetExperiment());
    }

    /// <inheritdoc />
    public void Populate(IEnumerable<Experiment> experiments, IEnumerable<string> instructionFiles)
    {
        this.instructionFiles = instructionFiles.ToList();
        RefreshView(experiments);
    }

    /// <inheritdoc />
    public void UpdateInstructionFiles(IEnumerable<string> instructionFiles)
    {
        this.instructionFiles = instructionFiles.ToList();
        presenters.ForEach(p => p.UpdateInstructionFiles(instructionFiles));
    }

    /// <summary>
    /// Refresh the view with the given experiments.
    /// </summary>
    /// <param name="experiments">The experiments to display.</param>
    private void RefreshView(IEnumerable<Experiment> experiments)
    {
        // Force greedy evaluation before clearing the presenters list.
        experiments = experiments.ToList();

        // Dispose of all existing presenters and their views.
        presenters.ForEach(p => p.Dispose());
        presenters.Clear();

        // Create a new set of views.
        foreach (Experiment experiment in experiments)
        {
            IExperimentView view = new ExperimentView();
            IExperimentPresenter presenter = new ExperimentPresenter(instructionFiles, experiment, view);
            presenter.OnRenamed.ConnectTo(n => OnExperimentRenamed(experiment, n));
            presenters.Add(presenter);
        }

        // Update the master experiments view with the new set of views.
        view.Populate(presenters.Select(p => (p.GetExperiment(), p.GetView())));
    }

    /// <summary>
    /// Create an experiment instance to be added to the collection. This is
    /// used when the user wants to add a new experiment.
    /// </summary>
    private Experiment CreateDefaultExperiment()
    {
        return new Experiment(
            "New Experiment",
            "Description",
            Configuration.Instance.GetDefaultRunner()?.Name ?? string.Empty,
            instructionFiles,
            [],
            new FactorialGenerator(true, [])
        );
    }

    /// <summary>
    /// Called when the user wants to add an experiment.
    /// </summary>
    private void OnAdd()
    {
        RefreshView(GetExperiments().Append(CreateDefaultExperiment()));
    }

    /// <summary>
    /// Called when the user wants to remove an experiment.
    /// </summary>
    /// <param name="experiment">The experiment to remove.</param>
    private void OnRemove(Experiment experiment)
    {
        if (!presenters.Select(p => p.GetExperiment()).Contains(experiment))
            // Should never happen.
            throw new InvalidOperationException($"Experiment '{experiment}' not found in experiments");

        RefreshView(GetExperiments().Except([experiment]));
    }


    /// <summary>
    /// Called when an experiment is renamed by the user.
    /// </summary>
    /// <param name="experiment">The experiment that was renamed.</param>
    /// <param name="name">The new name of the experiment.</param>
    private void OnExperimentRenamed(Experiment experiment, string name)
    {
        view.Rename(experiment, name);
    }
}
