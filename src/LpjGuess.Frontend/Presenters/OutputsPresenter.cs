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
        // Get the previously selected instruction file (if there is one).
        string? insFile = view.InstructionFile;

        // Populate the view. This will not fire an instruction file selected
        // event.
        view.PopulateInstructionFiles(instructionFiles);

        string? outputFileType;
        if (insFile != null && instructionFiles.Contains(insFile))
        {
            // Select the previously-selected instruction file.
            view.SelectInstructionFile(insFile);
            outputFileType = view.SelectedOutputFile?.Metadata.FileName;
        }
        else
        {
            // First item is selected by default.
            insFile = instructionFiles.FirstOrDefault();
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
        string? instructionFile = view.InstructionFile;
        if (instructionFile == null)
            // Shouldn't happen, but best to be safe.
            return;
        Simulation simulation = ModelOutputReader.GetSimulation(instructionFile);
        Task<Quantity> task = simulation.ReadOutputFileAsync(file.Path);
        task.ContinueWithOnMainThread(q =>
        {
            DataTable data = q.ToDataTable();
            view.PopulateData(data);
        });
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
        Simulation simulation = ModelOutputReader.GetSimulation(file);
        return simulation.GetOutputFiles();
    }

    /// <summary>
    /// Called when the user has selected an instruction file in the instruction
    /// files dropdown. Populates the output files dropdown with all available
    /// outputs for the given instruction file.
    /// </summary>
    /// <param name="file">The instruction file selected by the user.</param>
    private void OnInstructionFileSelected(string file)
    {
        view.PopulateOutputFiles(GetOutputFiles(file));
    }
}
