using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Parsers;
using LpjGuess.Core.Services;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for an experiment view.
/// </summary>
[RegisterPresenter(typeof(Experiment), typeof(IExperimentPresenter))]
public class ExperimentPresenter : PresenterBase<IExperimentView, Experiment>, IExperimentPresenter
{
    /// <summary>
    /// Maximum number of simulations materialised for the GUI preview.
    /// </summary>
    private const int previewLimit = 100;

    /// <summary>
    /// The factorial presenter.
    /// </summary>
    private readonly IFactorialPresenter factorialPresenter;

    /// <summary>
    /// The factory to use for creating presenters.
    /// </summary>
    private readonly IPresenterFactory presenterFactory;

    /// <summary>
    /// The available instruction files in the workspace.
    /// </summary>
    private IInstructionFilesProvider insFilesProvider;

    /// <inheritdoc />
    public Event<string> OnRenamed { get; private init; }

    /// <summary>
    /// Create a new <see cref="ExperimentPresenter"/> instance.
    /// </summary>
    /// <param name="experiment">The experiment to present.</param>
    /// <param name="insFilesProvider">The instruction files provider.</param>
    /// <param name="view">The view to present the experiment on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    /// <param name="presenterFactory">The factory to use for creating presenters.</param>
    public ExperimentPresenter(
        Experiment experiment,
        IInstructionFilesProvider insFilesProvider,
        IExperimentView view,
        ICommandRegistry registry,
        IPresenterFactory presenterFactory) : base(view, experiment, registry)
    {
        OnRenamed = new Event<string>();
        this.insFilesProvider = insFilesProvider;
        this.presenterFactory = presenterFactory;
        view.OnChanged.ConnectTo(OnExperimentChanged);
        factorialPresenter = presenterFactory.CreatePresenter<IFactorialPresenter, FactorialGenerator>(GetFactorialGenerator());
        factorialPresenter.SetInstructionFiles(GetEnabledInstructionFiles(
            insFilesProvider.GetInstructionFiles()));
        view.SetFactorialView(factorialPresenter.GetView());
        RefreshView();

        factorialPresenter.OnChanged.ConnectTo(OnSimulationGeneratorChanged);
        this.insFilesProvider.OnInstructionFilesChanged.ConnectTo(OnInsFilesChanged);
    }

    /// <inheritdoc />
    public Experiment GetExperiment() => model;

    /// <inheritdoc />
    public override void Dispose()
    {
        insFilesProvider.OnInstructionFilesChanged.DisconnectFrom(OnInsFilesChanged);
        factorialPresenter.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Refresh the view.
    /// </summary>
    private void RefreshView()
    {
        List<string> instructionFiles = insFilesProvider.GetInstructionFiles().ToList();
        factorialPresenter.SetInstructionFiles(GetEnabledInstructionFiles(instructionFiles));
        IReadOnlyList<(string Name, bool EnabledByDefault)> availablePfts =
            GetAvailablePfts(GetEnabledInstructionFiles(instructionFiles));
        bool inheritPfts = model.Pfts.Count == 0;
        view.Populate(
            model.Name,
            model.Description,
            model.Runner,
            model.InputModule,
            model.ExistingOutputPolicy,
            instructionFiles.Select(f => (f, !model.DisabledInsFiles.Contains(f))),
            inheritPfts,
            availablePfts.Select(p => (
                p.Name,
                p.EnabledByDefault,
                model.Pfts.Contains(p.Name))));
        UpdatePreview(instructionFiles);

        // Do we need to refresh the factorial presenter here? I think not.
    }

    /// <summary>
    /// Update the simulations table displayed in the view.
    /// </summary>
    private void UpdatePreview(IReadOnlyCollection<string> instructionFiles)
    {
        List<string> selectedFiles = GetEnabledInstructionFiles(instructionFiles).ToList();
        ExperimentDesignAnalysis analysis = ExperimentDesignAnalyser.Analyse(
            GetFactorialGenerator(),
            selectedFiles.Count);
        analysis = analysis with
        {
            Issues = analysis.Issues
                .Concat(ExperimentInstructionFileAnalyser.Analyse(
                    GetFactorialGenerator(),
                    selectedFiles,
                    model.Pfts))
                .ToList()
        };

        List<ISimulation> simulations = model.SimulationGenerator
            .Generate()
            .Take(previewLimit + 1)
            .ToList();
        bool truncated = simulations.Count > previewLimit;
        view.PopulatePreview(
            analysis,
            GetSimulationDescriptions(simulations.Take(previewLimit)),
            truncated);
    }

    /// <summary>
    /// Generate a description of all simulations generated by the experiment.
    /// </summary>
    /// <returns>A collection of simulation descriptions.</returns>
    private List<SimulationDescription> GetSimulationDescriptions(IEnumerable<ISimulation> simulations)
    {
        List<SimulationDescription> descriptions = new List<SimulationDescription>();

        foreach (Simulation simulation in simulations.OfType<Simulation>())
        {
            List<ParameterChange> changes = new List<ParameterChange>();
            foreach (IFactor factor in simulation.Changes)
            {
                string factorName = factor.GetName();
                foreach (ParameterOverride change in factor.GetParameterOverrides())
                    changes.Add(new ParameterChange(change.Target, change.Value, factorName));
            }

            descriptions.Add(new SimulationDescription(simulation.Name, changes));
        }

        return descriptions;
    }

    /// <summary>
    /// Discover PFT names from the instruction files currently in the workspace.
    /// Files which cannot be parsed are ignored here; their normal editor/run
    /// paths will still report the underlying parse error.
    /// </summary>
    private static IReadOnlyList<(string Name, bool EnabledByDefault)> GetAvailablePfts(
        IEnumerable<string> instructionFiles)
    {
        Dictionary<string, bool> pfts = new(StringComparer.OrdinalIgnoreCase);
        foreach (string file in instructionFiles.Where(File.Exists))
        {
            try
            {
                InstructionFileParser parser = InstructionFileParser.FromFile(file);
                foreach (string pft in parser.GetBlockNames("pft"))
                {
                    bool enabled = parser.GetBlockParameter("pft", pft, "include")
                        ?.TryGetInt(out int include) == true &&
                        include == 1;
                    pfts[pft] = pfts.GetValueOrDefault(pft) || enabled;
                }
            }
            catch
            {
                // Discovery is best-effort and must not prevent the experiment
                // configuration page from opening.
            }
        }
        return pfts
            .OrderBy(pair => pair.Key)
            .Select(pair => (pair.Key, pair.Value))
            .ToList();
    }

    private IEnumerable<string> GetEnabledInstructionFiles(IEnumerable<string> instructionFiles)
        => instructionFiles.Where(file => !model.DisabledInsFiles.Contains(file));

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
        if (model.SimulationGenerator is not FactorialGenerator generator)
            throw new NotImplementedException($"{model.SimulationGenerator.GetType().Name} is not yet supported.");
        return generator;
    }

    /// <summary>
    /// Invoke the specified command.
    /// </summary>
    /// <param name="command">The command to invoke.</param>
    protected override void InvokeCommand(ICommand command)
    {
        // This will become slightly less trivial once we have a command
        // history.
        string oldName = model.Name;
        base.InvokeCommand(command);
        if (model.Name != oldName)
            OnRenamed.Invoke(model.Name);
        RefreshView();
    }

    /// <summary>
    /// Called when an instruction file is added to or removed from the
    /// workspace.
    /// </summary>
    /// <param name="instructionFiles">The instruction files in the workspace.</param>
    private void OnInsFilesChanged(IEnumerable<string> instructionFiles)
    {
        RefreshView();
    }

    /// <summary>
    /// Called when the user wants to change the experiment.
    /// </summary>
    /// <param name="change">The change to apply.</param>
    private void OnExperimentChanged(IModelChange<Experiment> change)
    {
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }

    /// <summary>
    /// Called when the simulation generator changes.
    /// </summary>
    private void OnSimulationGeneratorChanged()
    {
        UpdatePreview(insFilesProvider.GetInstructionFiles().ToList());
    }
}
