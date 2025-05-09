using System.Data;
using ExtendedXmlSerializer;
using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which allows the user to view the raw outputs from the model.
/// </summary>
public class OutputsView : Box, IOutputsView
{
	/// <summary>
	/// Name of the property corresponding to the selected item in a dropdown.
	/// </summary>
	private const string selectedItemProperty = "selected-item";

	/// <summary>
	/// The model behind the instruction files dropdown box.
	/// </summary>
	private readonly StringList insFilesModel;

	/// <summary>
	/// The dropdown box containing the instruction files.
	/// </summary>
	private readonly DropDown insFilesDropdown;

	/// <summary>
	/// The model behind the output files dropdown box.
	/// </summary>
	private readonly StringList outputFilesModel;

	/// <summary>
	/// The dropdown box containing the output files.
	/// </summary>
	private readonly DropDown outputsDropdown;

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
		OnOutputFileSelected = new Event<string>();

		SetOrientation(Orientation.Vertical);

		Label insFilesLabel = Label.New("Instruction File:");
		insFilesLabel.Halign = Align.Start;
		insFilesLabel.Valign = Align.Center;

		insFilesDropdown = new DropDown();
		insFilesModel = StringList.New(Array.Empty<string>());
		insFilesDropdown.Model = insFilesModel;
		insFilesDropdown.Hexpand = true;
		insFilesDropdown.OnNotify += OnInsFileActivated;

		Label outputsLabel = Label.New("Output File:");
		outputsLabel.Halign = Align.Start;
		outputsLabel.Valign = Align.Center;

		outputsDropdown = new DropDown();
		outputFilesModel = StringList.New(Array.Empty<string>());
		outputsDropdown.Model = outputFilesModel;
		outputsDropdown.Hexpand = true;
		outputsDropdown.OnNotify += OnOutputActivated;

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
		grid.Attach(insFilesDropdown, 1, 0, 1, 1);
		grid.Attach(outputsLabel, 0, 1, 1, 1);
		grid.Attach(outputsDropdown, 1, 1, 1, 1);
		grid.Attach(dataScroller, 0, 2, 2, 1);

		Append(grid);
	}

	/// <inheritdoc />
	public string? InstructionFile
	{
		get
		{
			if (insFilesDropdown.SelectedItem is StringObject str)
				return str.String;
			return null;
		}
	}

    /// <inheritdoc />
    public Event<string> OnInstructionFileSelected { get; private init; }

    /// <inheritdoc />
    public Event<string> OnOutputFileSelected { get; private init; }

    /// <inheritdoc />
    public Widget GetWidget() => this;

    /// <inheritdoc />
    public void PopulateInstructionFiles(IEnumerable<string> instructionFiles)
    {
		// Remove everything from the model.
		insFilesDropdown.OnNotify -= OnInsFileActivated;
		uint n = insFilesModel.GetNItems();
        for (uint i = 0; i < n; i++)
			insFilesModel.Remove(0);
		insFilesDropdown.OnNotify += OnInsFileActivated;

		// Populate the model with the new collection of instruction files.
		foreach (string instructionFile in instructionFiles)
			insFilesModel.Append(instructionFile);
    }

    /// <inheritdoc />
    public void PopulateOutputFiles(IEnumerable<string> outputFiles)
    {
		// Remove everything from the model.
		outputsDropdown.OnNotify -= OnOutputActivated;
		uint n = outputFilesModel.GetNItems();
        for (uint i = 0; i < n; i++)
			outputFilesModel.Remove(0);
		outputsDropdown.OnNotify += OnOutputActivated;

		// Populate the model with the new collection of output files.
		foreach (string outputFile in outputFiles)
			outputFilesModel.Append(outputFile);
    }

	/// <inheritdoc />
    public void PopulateData(DataTable data)
    {
        dataView.Populate(data);
    }

	/// <summary>
	/// Called when the user has selected an output file from the dropdown box.
	/// Propagates the event to the presenter.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
    private void OnOutputActivated(GObject.Object sender, NotifySignalArgs args)
    {
        try
		{
			string property = args.Pspec.GetName();
			if (property == selectedItemProperty && outputsDropdown.SelectedItem is StringObject str && str.String != null)
				OnOutputFileSelected.Invoke(str.String);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
    }

	/// <summary>
	/// Called when the user has selected an instruction file from the dropdown
	/// box. Propagates the event to the presenter.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
    private void OnInsFileActivated(GObject.Object sender, NotifySignalArgs args)
    {
        try
		{
			string property = args.Pspec.GetName();
			if (property == selectedItemProperty && insFilesDropdown.SelectedItem is StringObject str && str.String != null)
				OnInstructionFileSelected.Invoke(str.String);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
    }
}
