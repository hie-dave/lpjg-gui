using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which manages the experiments.
/// </summary>
[RegisterPresenter(typeof(List<Experiment>), typeof(IExperimentsPresenter))]
public class ExperimentsPresenter : PresenterBase<IExperimentsView, List<Experiment>>, IExperimentsPresenter
{
    /// <summary>
    /// The presenter factory to use for creating experiment presenters.
    /// </summary>
    private readonly IPresenterFactory presenterFactory;

    /// <summary>
    /// The experiment provider.
    /// </summary>
    private readonly IExperimentProvider experimentProvider;

    /// <summary>
    /// The list of experiment presenters.
    /// </summary>
    private readonly List<IExperimentPresenter> presenters;

    /// <summary>
    /// Create a new <see cref="ExperimentsPresenter"/> instance.
    /// </summary>
    /// <param name="experiments">The experiments to present.</param>
    /// <param name="view">The view to present.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    /// <param name="presenterFactory">The presenter factory to use for creating experiment presenters.</param>
    /// <param name="experimentProvider">The experiment provider.</param>
    public ExperimentsPresenter(
        List<Experiment> experiments,
        IExperimentsView view,
        ICommandRegistry registry,
        IPresenterFactory presenterFactory,
        IExperimentProvider experimentProvider) : base(view, experiments, registry)
    {
        this.presenterFactory = presenterFactory;
        this.experimentProvider = experimentProvider;

        view.AddText = "Add Experiment";

        view.OnAdd.ConnectTo(OnAdd);
        view.OnRemove.ConnectTo(OnRemove);

        presenters = [];
        Refresh();
    }

    /// <inheritdoc />
    public List<Experiment> GetExperiments() => model;

    /// <inheritdoc />
    public void Refresh()
    {
        RefreshView(model);
    }

    /// <summary>
    /// Refresh the view with the given experiments.
    /// </summary>
    /// <param name="experiments">The experiments to display.</param>
    private void RefreshView(List<Experiment> experiments)
    {
        // Force greedy evaluation before clearing the presenters list.
        experiments = experiments.ToList();

        // Dispose of all existing presenters and their views.
        presenters.ForEach(p => p.Dispose());
        presenters.Clear();

        // Create a new set of views.
        foreach (Experiment experiment in experiments)
        {
            IExperimentPresenter presenter = presenterFactory.CreatePresenter<IExperimentPresenter, Experiment>(experiment);
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
            [],
            [],
            new FactorialGenerator(true, [])
        );
    }

    /// <summary>
    /// Called when the user wants to add an experiment.
    /// </summary>
    private void OnAdd()
    {
        Experiment experiment = CreateDefaultExperiment();
        AddElementCommand<Experiment> command = new(model, experiment);
        registry.Execute(command);
        RefreshView(model);

        // Propagate the change to any listening presenters.
        if (experimentProvider is ExperimentProvider provider)
            provider.UpdateExperiments(model);
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

        RemoveElementCommand<Experiment> command = new(model, experiment);
        registry.Execute(command);
        RefreshView(model);

        // Propagate the change to any listening presenters.
        if (experimentProvider is ExperimentProvider provider)
            provider.UpdateExperiments(model);
    }

    /// <summary>
    /// Called when an experiment is renamed by the user.
    /// </summary>
    /// <param name="experiment">The experiment that was renamed.</param>
    /// <param name="name">The new name of the experiment.</param>
    private void OnExperimentRenamed(Experiment experiment, string name)
    {
        PropertyChangeCommand<Experiment, string> command = new(experiment, experiment.Name, name, (e, n) => e.Name = n);
        registry.Execute(command);
        view.Rename(experiment, name);

        // Propagate the change to any listening presenters.
        if (experimentProvider is ExperimentProvider provider)
            provider.UpdateExperiments(model);
    }
}
