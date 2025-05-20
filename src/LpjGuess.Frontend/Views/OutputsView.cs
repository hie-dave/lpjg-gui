using System.Data;
using ExtendedXmlSerializer;
using Gtk;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which allows the user to view the raw outputs from the model.
/// </summary>
public class OutputsView : Box, IOutputsView
{
	/// <summary>
	/// The dropdown box containing the instruction files.
	/// </summary>
	private readonly DropDownView<string> insFilesDropdown;

	/// <summary>
	/// The dropdown box containing the output files.
	/// </summary>
	private readonly DropDownView<OutputFile> outputsDropdown;

	/// <summary>
	/// The view responsible for displaying data to the user.
	/// </summary>
	private readonly DataTableView dataView;

	/// <summary>
	/// Create a new <see cref="OutputsView"/> instance.
	/// </summary>
	public OutputsView()
	{
		OnInstructionFileSelected = new Event<string>();
		OnOutputFileSelected = new Event<OutputFile>();

		SetOrientation(Orientation.Vertical);

		Label insFilesLabel = Label.New("Instruction File:");
		insFilesLabel.Halign = Align.Start;
		insFilesLabel.Valign = Align.Center;

		insFilesDropdown = new DropDownView<string>();
		insFilesDropdown.GetWidget().Hexpand = true;
		insFilesDropdown.OnSelectionChanged.ConnectTo(OnInstructionFileSelected);

		Label outputsLabel = Label.New("Output File:");
		outputsLabel.Halign = Align.Start;
		outputsLabel.Valign = Align.Center;

		outputsDropdown = new DropDownView<OutputFile>();
		outputsDropdown.GetWidget().Hexpand = true;
		outputsDropdown.OnSelectionChanged.ConnectTo(OnOutputFileSelected);

		dataView = new DataTableView();
		dataView.Hexpand = true;
		dataView.Vexpand = true;
		ScrolledWindow dataScroller = new ScrolledWindow();
		dataScroller.Child = dataView;

		Grid grid = new Grid();
		grid.RowSpacing = 12;
		grid.ColumnSpacing = 12;
		grid.MarginBottom = 6;
		grid.MarginTop = 6;
		grid.MarginStart = 6;
		grid.MarginEnd = 6;
		grid.Attach(insFilesLabel, 0, 0, 1, 1);
		grid.Attach(insFilesDropdown.GetWidget(), 1, 0, 1, 1);
		grid.Attach(outputsLabel, 0, 1, 1, 1);
		grid.Attach(outputsDropdown.GetWidget(), 1, 1, 1, 1);
		grid.Attach(dataScroller, 0, 2, 2, 1);

		Append(grid);
	}

	/// <inheritdoc />
	public string? InstructionFile => insFilesDropdown.Selection;

    /// <inheritdoc />
    public Event<string> OnInstructionFileSelected { get; private init; }

    /// <inheritdoc />
    public Event<OutputFile> OnOutputFileSelected { get; private init; }

    /// <inheritdoc />
    public Widget GetWidget() => this;

    /// <inheritdoc />
    public void PopulateInstructionFiles(IEnumerable<string> instructionFiles)
    {
		// Remove everything from the model.
		insFilesDropdown.Populate(instructionFiles, Path.GetFileName);
    }

    /// <inheritdoc />
    public void PopulateOutputFiles(IEnumerable<OutputFile> outputFiles)
    {
		outputsDropdown.Populate(outputFiles, o => o.Metadata.Description);
    }

	/// <inheritdoc />
    public void PopulateData(DataTable data)
    {
        dataView.Populate(data);
    }
}
