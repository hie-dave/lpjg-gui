using LpjGuess.Core.Models.Importer;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Services;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which controls an outputs view to display the raw outputs from a
/// model run.
/// </summary>
[RegisterStandalonePresenter(typeof(IOutputsPresenter))]
public class OutputsPresenter : IOutputsPresenter
{
    /// <summary>
    /// The view object.
    /// </summary>
    private readonly IOutputsView view;

    /// <summary>
    /// The experiment provider.
    /// </summary>
    private readonly IExperimentProvider provider;

    /// <summary>
    /// The path resolver.
    /// </summary>
    private readonly IPathResolver pathResolver;

    private IInstructionFilesProvider instructionFilesProvider;

    /// <summary>
    /// The cancellation token source for the output file parsing task.
    /// </summary>
    private CancellationTokenSource cts;

    /// <summary>
    /// Create a new <see cref="OutputsPresenter"/> instance.
    /// </summary>
    /// <param name="view">The view object.</param>
    /// <param name="provider">The experiment provider.</param>
    /// <param name="pathResolver">The path resolver.</param>
    /// <param name="instructionFilesProvider">The instruction files provider.</param>
    public OutputsPresenter(
        IOutputsView view,
        IExperimentProvider provider,
        IPathResolver pathResolver,
        IInstructionFilesProvider instructionFilesProvider)
    {
        this.view = view;
        this.provider = provider;
        this.pathResolver = pathResolver;
        this.instructionFilesProvider = instructionFilesProvider;
        view.OnExperimentSelected.ConnectTo(OnExperimentSelected);
        view.OnSimulationSelected.ConnectTo(OnSimulationSelected);
        view.OnInstructionFileSelected.ConnectTo(OnInstructionFileSelected);
        view.OnOutputFileSelected.ConnectTo(OnOutputFileSelected);
        cts = new CancellationTokenSource();

        // Call UpdateExperiments any time an experiment is added or removed
        // to or from the workspace.
        this.provider.OnExperimentsChanged.ConnectTo(UpdateExperiments);
        this.instructionFilesProvider.OnInstructionFilesChanged.ConnectTo(UpdateInstructionFiles);

        try
        {
            UpdateExperiments();
            UpdateSimulations();
            UpdateInstructionFiles();
            UpdateOutputFiles();
            RefreshData();
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Updates the experiments dropdown with the currently-available experiments.
    /// </summary>
    private void UpdateExperiments()
    {
        IEnumerable<string> experiments = provider.GetExperiments().Select(e => e.Name);
        view.PopulateExperiments(experiments);
    }

    /// <summary>
    /// Updates the simulations dropdown with the currently-available simulations.
    /// </summary>
    private void UpdateSimulations()
    {
        string? experimentName = view.SelectedExperiment;
        if (experimentName == null)
            return;

        IEnumerable<string> simulations = GetSimulations(experimentName);
        view.PopulateSimulations(simulations);
    }

    /// <summary>
    /// Updates the instruction files dropdown with the currently-available
    /// instruction files.
    /// </summary>
    private void UpdateInstructionFiles()
    {
        string? experimentName = view.SelectedExperiment;
        if (experimentName == null)
            return;

        Experiment experiment = GetExperiment(experimentName);
        IEnumerable<string> instructionFiles = GetInstructionFiles(experiment);
        view.PopulateInstructionFiles(instructionFiles);
    }

    /// <summary>
    /// Updates the output files dropdown with the currently-available output files.
    /// </summary>
    private void UpdateOutputFiles()
    {
        string? experimentName = view.SelectedExperiment;
        string? simulationName = view.SelectedSimulation;
        string? insFile = view.InstructionFile;
        if (experimentName == null || simulationName == null || insFile == null)
            return;

        Experiment experiment = GetExperiment(experimentName);
        ISimulation simulation = GetSimulation(experiment, simulationName);
        string concreteInsFile = pathResolver.GenerateTargetInsFilePath(insFile, simulation);
        InstructionFile ins = new(concreteInsFile, experimentName, simulationName);
        IEnumerable<OutputFile> outputFiles = GetOutputFiles(ins);
        view.PopulateOutputFiles(outputFiles);
    }

    /// <inheritdoc />
    public void RefreshData()
    {
        string? experimentName = view.SelectedExperiment;
        string? simulationName = view.SelectedSimulation;
        string? insFile = view.InstructionFile;
        OutputFile? outputFile = view.SelectedOutputFile;
        if (experimentName == null || simulationName == null || insFile == null || outputFile == null)
            return;

        OnOutputFileSelected(outputFile);
    }

    /// <inheritdoc/>
    public IView GetView() => view;

    /// <inheritdoc/>
    public void Dispose()
    {
        view.Dispose();
    }

    /// <summary>
    /// Handle the instruction file selected event by discovering the output
    /// files available for the given instruction file.
    /// </summary>
    /// <param name="file">The instruction file selected by the user.</param>
    /// <returns>The output files available for the given instruction file.</returns>
    private static IEnumerable<OutputFile> GetOutputFiles(InstructionFile file)
    {
        if (!File.Exists(file.FileName))
            return [];

        // TODO: consolidate instruction file parsers in runner/benchmarks.
        // We are double parsing here (since we also parse when ins files are
        // selected by the user).
        SimulationReader simulation = ModelOutputReader.GetSimulation(file);
        return simulation.GetOutputFiles();
    }

    /// <summary>
    /// Gets the simulations available for the specified experiment.
    /// </summary>
    /// <param name="experimentName">The name of the experiment.</param>
    private IEnumerable<string> GetSimulations(string experimentName)
    {
        Experiment? experiment = GetExperiment(experimentName);
        return experiment.SimulationGenerator.Generate().Select(s => s.Name);
    }

    /// <summary>
    /// Gets the instruction files available for the specified experiment.
    /// </summary>
    /// <param name="experiment">The experiment to search within.</param>
    private IEnumerable<string> GetInstructionFiles(Experiment experiment)
    {
        return instructionFilesProvider.GetInstructionFiles()
            .Except(experiment.DisabledInsFiles);
    }

    /// <summary>
    /// Gets the experiment with the specified name.
    /// </summary>
    /// <param name="name">The name of the experiment.</param>
    /// <returns>The experiment with the specified name.</returns>
    private Experiment GetExperiment(string name)
    {
        Experiment? experiment = provider.GetExperiments().FirstOrDefault(e => e.Name == name);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment '{name}' not found");
        return experiment;
    }

    /// <summary>
    /// Gets the simulation with the specified name.
    /// </summary>
    /// <param name="experiment">The experiment to search within.</param>
    /// <param name="simulationName">The name of the simulation.</param>
    /// <returns>The simulation with the specified name.</returns>
    private ISimulation GetSimulation(Experiment experiment, string simulationName)
    {
        ISimulation? simulation = experiment.SimulationGenerator.Generate().FirstOrDefault(s => s.Name == simulationName);
        if (simulation == null)
            throw new InvalidOperationException($"Simulation '{simulationName}' not found");
        return simulation;
    }

    /// <summary>
    /// Called when the user has selected an output file in the output files
    /// dropdown. Populates the data area with the data from the specified
    /// output file.
    /// </summary>
    /// <param name="file">The output file selected by the user.</param>
    private void OnOutputFileSelected(OutputFile file)
    {
        string? experimentName = view.SelectedExperiment;
        if (experimentName == null)
            return;

        Experiment experiment = GetExperiment(experimentName);
        string? simulationName = view.SelectedSimulation;
        if (simulationName == null)
            return;

        string? insFile = view.InstructionFile;
        if (insFile == null)
            return;

        ISimulation simulation = GetSimulation(experiment, simulationName);
        string concreteInsFile = pathResolver.GenerateTargetInsFilePath(insFile, simulation);

        InstructionFile ins = new(concreteInsFile, experimentName, simulationName);
        SimulationReader reader = ModelOutputReader.GetSimulation(ins);

        // Cancel any existing tasks.
        cts.Cancel();
        cts = new CancellationTokenSource();

        // TODO: true async support.
        Task<Quantity> task = reader.ReadOutputFileAsync(file.Path, cts.Token);
        task.ContinueWith(q => q.Result.ToDataTable())
            .ContinueWithOnMainThread(view.PopulateData);
    }

    /// <summary>
    /// Called when the user has selected an instruction file in the instruction
    /// files dropdown. Populates the output files dropdown with all available
    /// outputs for the given instruction file.
    /// </summary>
    /// <param name="experiment">The experiment selected by the user.</param>
    private void OnExperimentSelected(string experiment)
    {
        UpdateSimulations();
        UpdateInstructionFiles();
        RefreshData();
    }

    /// <summary>
    /// Called when the user has selected a simulation in the simulations
    /// dropdown.
    /// </summary>
    /// <param name="simulationName">The simulation selected by the user.</param>
    private void OnSimulationSelected(string simulationName)
    {
        UpdateOutputFiles();
        RefreshData();
    }

    /// <summary>
    /// Called when the user has selected an instruction file in the instruction
    /// files dropdown.
    /// </summary>
    /// <param name="instructionFile">The instruction file selected by the user.</param>
    private void OnInstructionFileSelected(string instructionFile)
    {
        UpdateOutputFiles();
        RefreshData();
    }
}
