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
    /// The cancellation token source for the output file parsing task.
    /// </summary>
    private CancellationTokenSource cts;

    /// <summary>
    /// Create a new <see cref="OutputsPresenter"/> instance.
    /// </summary>
    /// <param name="view">The view object.</param>
    /// <param name="provider">The experiment provider.</param>
    public OutputsPresenter(
        IOutputsView view,
        IExperimentProvider provider)
    {
        this.view = view;
        this.provider = provider;
        view.OnExperimentSelected.ConnectTo(OnExperimentSelected);
        view.OnSimulationSelected.ConnectTo(OnSimulationSelected);
        view.OnOutputFileSelected.ConnectTo(OnOutputFileSelected);
        cts = new CancellationTokenSource();
        Refresh();
    }

    /// <inheritdoc />
    public void Refresh()
    {
        // Get the previously selected instruction file (if there is one).
        string? insFile = view.InstructionFile;

        IReadOnlyList<string> insFiles = insFilesProvider.GetInstructionFiles().ToList();

        // Populate the view. This will not fire an instruction file selected
        // event.
        view.PopulateInstructionFiles(insFiles);

        string? outputFileType;
        if (insFile != null && insFiles.Contains(insFile))
        {
            // Select the previously-selected instruction file.
            view.SelectInstructionFile(insFile);
            outputFileType = view.SelectedOutputFile?.Metadata.FileName;
        }
        else
        {
            // First item is selected by default.
            insFile = insFiles.FirstOrDefault();
            outputFileType = null;
        }

        // We need to update the output files dropdown. Note: insFile will be
        // null here if there was no previously-selected file, and if the
        // current collection of instruction files is empty.
        if (insFile != null)
        {
            IEnumerable<OutputFile> outputFiles = GetOutputFiles(insFile);
            view.PopulateOutputFiles(outputFiles);

            OutputFile? outputFile;
            if (outputFileType != null && outputFiles.Any(f => f.Metadata.FileName == outputFileType))
            {
                // User had previously selected an output file which is still
                // available. We should select this file.
                outputFile = outputFiles.First(f => f.Metadata.FileName == outputFileType);
                view.SelectOutputFile(outputFile);
            }
            else
            {
                // Either no output file was previously selected, or the
                // previously-selected file is no longer available.
                outputFile = outputFiles.FirstOrDefault();
                // No need to select this output file - the first item in the
                // collection was automatically selected when we called
                // Populate().
            }

            // We should update the data displayed in the view, if an output
            // file is now selected (which should be the case unless the
            // collection is empty).
            if (outputFile != null)
                OnOutputFileSelected(outputFile);
        }
    }

    /// <summary>
    /// Called when the user has selected an output file in the output files
    /// dropdown. Populates the data area with the data from the specified
    /// output file.
    /// </summary>
    /// <param name="file">The output file selected by the user.</param>
    private void OnOutputFileSelected(OutputFile file)
    {
        // TODO: true async support.
        string? experimentName = view.SelectedExperiment;
        if (experimentName == null)
            // Shouldn't happen, but best to be safe.
            return;

        Experiment experiment = GetExperiment(experimentName);
        string? simulationName = view.SelectedSimulation;
        if (simulationName == null)
            // Shouldn't happen, but best to be safe.
            return;

        ISimulation simulation = GetSimulation(experiment, simulationName);

        SimulationReader simulation = ModelOutputReader.GetSimulation(instructionFile);

        // Cancel any existing tasks.
        cts.Cancel();
        cts = new CancellationTokenSource();

        Task<Quantity> task = simulation.ReadOutputFileAsync(file.Path, cts.Token);
        task.ContinueWith(q => q.Result.ToDataTable())
            .ContinueWithOnMainThread(view.PopulateData);
    }

    /// <summary>
    /// Handle the instruction file selected event by discovering the output
    /// files available for the given instruction file.
    /// </summary>
    /// <param name="file">The instruction file selected by the user.</param>
    /// <returns>The output files available for the given instruction file.</returns>
    private IEnumerable<OutputFile> GetOutputFiles(string file)
    {
        // TODO: consolidate instruction file parsers in runner/benchmarks.
        // We are double parsing here (since we also parse when ins files are
        // selected by the user).
        SimulationReader simulation = ModelOutputReader.GetSimulation(file);
        return simulation.GetOutputFiles();
    }

    /// <summary>
    /// Called when the user has selected an instruction file in the instruction
    /// files dropdown. Populates the output files dropdown with all available
    /// outputs for the given instruction file.
    /// </summary>
    /// <param name="experiment">The experiment selected by the user.</param>
    private void OnExperimentSelected(string experiment)
    {
        view.PopulateSimulations(GetSimulations(experiment));
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

    /// <inheritdoc/>
    public IView GetView() => view;

    /// <inheritdoc/>
    public void Dispose()
    {
        view.Dispose();
    }
}
