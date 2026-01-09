using System.Data;
using Gtk;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Views;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which allows the user to view the raw outputs from the model.
/// </summary>
public class OutputsView : Box, IOutputsView
{
	/// <summary>
	/// The dropdown box containing the instruction files.
	/// </summary>
	private readonly StringDropDownView<string> experimentsDropdown;

	/// <summary>
	/// The dropdown box containing the simulation files.
	/// </summary>
	private readonly StringDropDownView<string> simulationsDropdown;

	/// <summary>
	/// The dropdown box containing the instruction files.
	/// </summary>
	private readonly StringDropDownView<string> instructionFilesDropdown;

	/// <summary>
	/// The dropdown box containing the output files.
	/// </summary>
	private readonly OutputFilesDropDownView outputsDropdown;

	/// <summary>
	/// The view responsible for displaying data to the user.
	/// </summary>
	private readonly DataTableView dataView;

	/// <summary>
	/// Create a new <see cref="OutputsView"/> instance.
	/// </summary>
	/// <param name="factory">The logger factory.</param>
	public OutputsView(ILoggerFactory factory)
	{
		OnExperimentSelected = new Event<string>();
		OnSimulationSelected = new Event<string>();
		OnInstructionFileSelected = new Event<string>();
		OnOutputFileSelected = new Event<OutputFile>();

		SetOrientation(Orientation.Vertical);

		Label experimentsLabel = Label.New("Experiment:");
		experimentsLabel.Halign = Align.Start;
		experimentsLabel.Valign = Align.Center;

		experimentsDropdown = new StringDropDownView<string>(Path.GetFileName);
		experimentsDropdown.GetWidget().Hexpand = true;
		experimentsDropdown.OnSelectionChanged.ConnectTo(OnExperimentSelected);

		Label simulationsLabel = Label.New("Simulation:");
		simulationsLabel.Halign = Align.Start;
		simulationsLabel.Valign = Align.Center;

		simulationsDropdown = new StringDropDownView<string>(Path.GetFileName);
		simulationsDropdown.GetWidget().Hexpand = true;
		simulationsDropdown.OnSelectionChanged.ConnectTo(OnSimulationSelected);

		Label instructionFilesLabel = Label.New("Instruction File:");
		instructionFilesLabel.Halign = Align.Start;
		instructionFilesLabel.Valign = Align.Center;

		instructionFilesDropdown = new StringDropDownView<string>(Path.GetFileName);
		instructionFilesDropdown.GetWidget().Hexpand = true;
		instructionFilesDropdown.OnSelectionChanged.ConnectTo(OnInstructionFileSelected);

		Label outputsLabel = Label.New("Output File:");
		outputsLabel.Halign = Align.Start;
		outputsLabel.Valign = Align.Center;

		outputsDropdown = new OutputFilesDropDownView();
		outputsDropdown.GetWidget().Hexpand = true;
		outputsDropdown.OnDataItemSelected.ConnectTo(OnOutputFileSelected);

		dataView = new DataTableView(factory.CreateLogger<DataTableView>());
		dataView.Hexpand = true;
		dataView.Vexpand = true;

		// Populate with a tiny placeholder so the ColumnView doesn't start with
		// zero columns/rows, which can trigger gtk_adjustment_configure warnings
		// when embedded in a ScrolledWindow during initial allocation.
		var placeholder = new DataTable();
		placeholder.Columns.Add(" ");
		var placeholderRow = placeholder.NewRow();
		placeholder.Rows.Add(placeholderRow);
		dataView.Populate(placeholder);

		ScrolledWindow dataScroller = new ScrolledWindow();
		dataScroller.Hexpand = true;
		dataScroller.Vexpand = true;
		dataScroller.Child = dataView;

		Grid grid = new Grid();
		grid.RowSpacing = 12;
		grid.ColumnSpacing = 12;
		grid.MarginBottom = 6;
		grid.MarginTop = 6;
		grid.MarginStart = 6;
		grid.MarginEnd = 6;
		grid.Attach(experimentsLabel, 0, 0, 1, 1);
		grid.Attach(experimentsDropdown.GetWidget(), 1, 0, 1, 1);
		grid.Attach(simulationsLabel, 0, 1, 1, 1);
		grid.Attach(simulationsDropdown.GetWidget(), 1, 1, 1, 1);
		grid.Attach(instructionFilesLabel, 0, 2, 1, 1);
		grid.Attach(instructionFilesDropdown.GetWidget(), 1, 2, 1, 1);
		grid.Attach(outputsLabel, 0, 3, 1, 1);
		grid.Attach(outputsDropdown.GetWidget(), 1, 3, 1, 1);
		grid.Attach(dataScroller, 0, 4, 2, 1);

		Append(grid);
	}

	/// <inheritdoc />
	public string? SelectedExperiment => experimentsDropdown.Selection;

	/// <inheritdoc />
	public string? SelectedSimulation => simulationsDropdown.Selection;

	/// <inheritdoc />
	public OutputFile? SelectedOutputFile => outputsDropdown.Selection;

	/// <inheritdoc />
	public string? InstructionFile => instructionFilesDropdown.Selection;

	/// <inheritdoc />
	public Event<string> OnExperimentSelected { get; private init; }

	/// <inheritdoc />
	public Event<string> OnSimulationSelected { get; private init; }

	/// <inheritdoc />
	public Event<string> OnInstructionFileSelected { get; private init; }

    /// <inheritdoc />
    public Event<OutputFile> OnOutputFileSelected { get; private init; }

    /// <inheritdoc />
    public Widget GetWidget() => this;

    /// <inheritdoc />
    public void PopulateExperiments(IEnumerable<string> experiments)
    {
		// Remove everything from the model.
		experimentsDropdown.Populate(experiments);
    }

	/// <inheritdoc />
	public void PopulateSimulations(IEnumerable<string> simulations)
	{
		// Remove everything from the model.
		simulationsDropdown.Populate(simulations);
	}

	/// <inheritdoc />
	public void PopulateInstructionFiles(IEnumerable<string> instructionFiles)
	{
		instructionFilesDropdown.Populate(instructionFiles);
	}

	/// <inheritdoc />
	public void PopulateOutputFiles(IEnumerable<OutputFile> outputFiles)
	{
		outputsDropdown.Populate(outputFiles);
	}

	/// <inheritdoc />
    public void PopulateData(DataTable data)
    {
        dataView.Populate(data);
    }

	/// <inheritdoc />
	public void SelectExperiment(string experiment)
	{
		experimentsDropdown.Select(experiment);
	}

	/// <inheritdoc />
	public void SelectSimulation(string simulation)
	{
		simulationsDropdown.Select(simulation);
	}

	/// <inheritdoc />
	public void SelectInstructionFile(string instructionFile)
	{
		instructionFilesDropdown.Select(instructionFile);
	}

	/// <inheritdoc />
	public void SelectOutputFile(OutputFile file)
	{
		outputsDropdown.Select(file);
		dataView.Clear();
	}
}
