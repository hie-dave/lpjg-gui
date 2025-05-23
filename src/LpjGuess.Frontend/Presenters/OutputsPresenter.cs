using System.Data;
using Dave.Benchmarks.Core.Models.Importer;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which controls an outputs view to display the raw outputs from a
/// model run.
/// </summary>
public class OutputsPresenter : PresenterBase<IOutputsView>, IOutputsPresenter
{
    /// <summary>
    /// Create a new <see cref="OutputsPresenter"/> instance.
    /// </summary>
    /// <param name="view">The view object.</param>
    public OutputsPresenter(IOutputsView view) : base(view)
    {
        view.OnInstructionFileSelected.ConnectTo(OnInstructionFileSelected);
        view.OnOutputFileSelected.ConnectTo(OnOutputFileSelected);
    }

    /// <inheritdoc />
    public void Populate(IEnumerable<string> instructionFiles)
    {
        // Populate the view.
        view.PopulateInstructionFiles(instructionFiles);
        // TODO: should we select an instruction file?
        // TODO: should we select an output file?
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
        string? instructionFile = view.InstructionFile;
        if (instructionFile == null)
            // Shouldn't happen, but best to be safe.
            return;
        Simulation simulation = ModelOutputReader.GetSimulation(instructionFile);
        Task<Quantity> task = simulation.ReadOutputFileAsync(file.Path);
        task.Wait();

        Quantity quantity = task.Result;
        DataTable data = quantity.ToDataTable();
        view.PopulateData(data);
    }

    /// <summary>
    /// Called when the user has selected an instruction file in the instruction
    /// files dropdown. Populates the output files dropdown with all available
    /// outputs for the given instruction file.
    /// </summary>
    /// <param name="file">The instruction file selected by the user.</param>
    private void OnInstructionFileSelected(string file)
    {
        // TODO: consolidate instruction file parsers in runner/benchmarks.
        // We are double parsing here (since we also parse when ins files are
        // selected by the user).
        Simulation simulation = ModelOutputReader.GetSimulation(file);
        List<OutputFile> outputFiles = simulation.GetOutputFiles().ToList();
        view.PopulateOutputFiles(outputFiles);
    }
}
