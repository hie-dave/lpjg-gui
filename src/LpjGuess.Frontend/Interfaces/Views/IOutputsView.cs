using System.Data;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Interface to a view which renders model outputs in a tabular display.
/// </summary>
public interface IOutputsView : IView
{
    /// <summary>
    /// Get the currently-selected experiment.
    /// </summary>
    string? SelectedExperiment { get; }

    /// <summary>
    /// Get the currently-selected simulation.
    /// </summary>
    string? SelectedSimulation { get; }

    /// <summary>
    /// Get the currently-selected output file.
    /// </summary>
    OutputFile? SelectedOutputFile { get; }

    /// <summary>
    /// Invoked when the user has selected an experiment.
    /// </summary>
    Event<string> OnExperimentSelected { get; }

    /// <summary>
    /// Invoked when the user has selected a simulation.
    /// </summary>
    Event<string> OnSimulationSelected { get; }

    /// <summary>
    /// Invoked when the user has selected an output file.
    /// </summary>
    Event<OutputFile> OnOutputFileSelected { get; }

    /// <summary>
    /// Populate the view with the given experiment names.
    /// </summary>
    /// <param name="experimentNames">Experiment names.</param>
    void PopulateExperiments(IEnumerable<string> experimentNames);

    /// <summary>
    /// Populate the view with the given simulation names.
    /// </summary>
    /// <param name="simulationNames">Simulation names.</param>
    void PopulateSimulations(IEnumerable<string> simulationNames);

    /// <summary>
    /// Populate the view with the given output files.
    /// </summary>
    /// <param name="outputFiles">Output file paths.</param>
    void PopulateOutputFiles(IEnumerable<OutputFile> outputFiles);

    /// <summary>
    /// Populate the tabular data widget the contents of an output file.
    /// </summary>
    /// <param name="data">The data to be displayed.</param>
    void PopulateData(DataTable data);

    /// <summary>
    /// Select the specified experiment.
    /// </summary>
    void SelectExperiment(string experiment);

    /// <summary>
    /// Select the specified simulation.
    /// </summary>
    void SelectSimulation(string simulation);

    /// <summary>
    /// Select the specified output file.
    /// </summary>
    void SelectOutputFile(OutputFile file);
}
