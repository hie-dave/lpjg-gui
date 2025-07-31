using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views;
using LpjGuess.Runner.Models;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which manages the experiments.
/// </summary>
[RegisterPresenter(typeof(IEnumerable<Experiment>), typeof(IExperimentsPresenter))]
public class ExperimentsPresenter : PresenterBase<IExperimentsView, IEnumerable<Experiment>>, IExperimentsPresenter
{
    /// <summary>
    /// The presenter factory to use for creating experiment presenters.
    /// </summary>
    private readonly IPresenterFactory presenterFactory;

    /// <summary>
    /// The list of experiment presenters.
    /// </summary>
    private List<IExperimentPresenter> presenters;

    /// <summary>
    /// Create a new <see cref="ExperimentsPresenter"/> instance.
    /// </summary>
    /// <param name="experiments">The experiments to present.</param>
    /// <param name="view">The view to present.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    /// <param name="presenterFactory">The presenter factory to use for creating experiment presenters.</param>
    public ExperimentsPresenter(
        IEnumerable<Experiment> experiments,
        IExperimentsView view,
        ICommandRegistry registry,
        IPresenterFactory presenterFactory) : base(view, experiments, registry)
    {
        this.presenterFactory = presenterFactory;

        view.AddText = "Add Experiment";

        view.OnAdd.ConnectTo(OnAdd);
        view.OnRemove.ConnectTo(OnRemove);

        presenters = [];
        Refresh();
    }

    /// <inheritdoc />
    public IEnumerable<Experiment> GetExperiments()
    {
        return presenters.Select(p => p.GetExperiment());
    }

    /// <inheritdoc />
    public void Refresh()
    {
        RefreshView(model);
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
