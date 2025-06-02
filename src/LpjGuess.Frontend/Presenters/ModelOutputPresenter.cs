using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Dave.Benchmarks.Core.Models;
using Dave.Benchmarks.Core.Models.Entities;
using Dave.Benchmarks.Core.Models.Importer;
using Dave.Benchmarks.Core.Services;
using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which controls a data source view to allow the user to view and
/// customize a single data source.
/// </summary>
public class ModelOutputPresenter : PresenterBase<IModelOutputView>, IDataSourcePresenter
{
    /// <summary>
    /// The instruction files in the workspace.
    /// </summary>
    private readonly IEnumerable<string> instructionFiles;

    /// <inheritdoc/>
    public ModelOutput DataSource { get; private init; }

    /// <inheritdoc/>
    public Event<ICommand> OnDataSourceChanged { get; private init; }

    /// <inheritdoc/>
    IDataSource IDataSourcePresenter.DataSource => DataSource;

    /// <inheritdoc/>
    IDataSourceView IDataSourcePresenter.GetView() => view;

    /// <summary>
    /// Create a new <see cref="ModelOutputPresenter"/> instance.
    /// </summary>
    /// <param name="view">The view to present to.</param>
    /// <param name="dataSource">The data source being edited.</param>
    /// <param name="instructionFiles">The instruction files in the workspace.</param>
    public ModelOutputPresenter(
        IModelOutputView view,
        ModelOutput dataSource,
        IEnumerable<string> instructionFiles) : base(view)
    {
        DataSource = dataSource;
        OnDataSourceChanged = new Event<ICommand>();
        view.OnEditDataSource.ConnectTo(OnEditDataSource);
        view.OnFileTypeChanged.ConnectTo(OnFileTypeChanged);
        this.instructionFiles = instructionFiles;

        RefreshView();
    }

    /// <summary>
    /// Refresh the data displayed in the view with that in the model object.
    /// </summary>
    public void RefreshView()
    {
        IEnumerable<OutputFile> fileTypes = instructionFiles
            .Select(ModelOutputReader.GetSimulation)
            .SelectMany(s => s.GetOutputFiles())
            .DistinctBy(o => o.Metadata.FileName);

        OutputFile? outputFileType = fileTypes
            .FirstOrDefault(o => o.Metadata.FileName == DataSource.OutputFileType);

        if (outputFileType == null)
        {
            // TODO: think about exception handling (this can throw).
            OutputFileMetadata meta = OutputFileDefinitions.GetMetadata(DataSource.OutputFileType);
            outputFileType = new OutputFile(meta, string.Empty);
            if (!fileTypes.Any(f => f.Metadata.FileName == meta.FileName))
                fileTypes = fileTypes.Append(outputFileType);
        }

        IEnumerable<string> columns = GetColumns(DataSource.OutputFileType)
                .Append(DataSource.XAxisColumn)
                .Append(DataSource.YAxisColumn)
                .Distinct();

        view.Populate(
            fileTypes,
            columns,
            outputFileType,
            DataSource.XAxisColumn,
            DataSource.YAxisColumn);
    }

    private IEnumerable<string> GetColumns(string fileType)
    {
        IEnumerable<string> fileTypes = instructionFiles
            .Select(ModelOutputReader.GetSimulation)
            .SelectMany(s => s.GetOutputFiles())
            .DistinctBy(o => o.Metadata.FileName)
            .Select(o => o.Metadata.FileName);

        List<string> columns = ["Date", "Lat", "Lon"];

        if (!fileTypes.Contains(fileType))
            // Unsure if this is possible.
            return columns;

        // Partially parse output file to get columns.
        // TODO: make this cancellable?
        IEnumerable<Task<IEnumerable<LayerMetadata>>> tasks = instructionFiles
            .Select(ModelOutputReader.GetSimulation)
            .Select(s => s.ReadOutputFileMetadataAsync(fileType));
        Task.WaitAll(tasks);
        IEnumerable<LayerMetadata> metadata = tasks.SelectMany(t => t.Result).Distinct();

        // All  output files should have date, latitude, and longitude.
        columns.AddRange(metadata.Select(l => l.Name));

        // FIXME: patch-level outputs will not output a patch column if the
        // simulation contains only a single patch. It seemed like such a
        // clever idea at the time too...
        OutputFileMetadata meta = OutputFileDefinitions.GetMetadata(fileType);
        if (meta.Level  > AggregationLevel.Gridcell)
            columns.Add("stand");
        if (meta.Level > AggregationLevel.Stand)
            columns.Add("patch");
        if (meta.Level > AggregationLevel.Patch)
            columns.Add("indiv");

        return columns.Distinct().ToList();
    }

    /// <summary>
    /// Called when the data source has been changed by the user.
    /// </summary>
    /// <param name="change">The action to perform on the data source.</param>
    private void OnEditDataSource(IModelChange<ModelOutput> change)
    {
        ICommand command = change.ToCommand(DataSource);
        OnDataSourceChanged.Invoke(command);
    }

    /// <summary>
    /// Called when the user changes the output file type.
    /// </summary>
    /// <param name="fileType">The new output file type.</param>
    private void OnFileTypeChanged(OutputFile fileType)
    {
        GuessColumns(fileType.Metadata.FileName, out string xcol, out string ycol);

        // Changing the file type will invalidate the x and y columns. Therefore
        // we need to create a composite command to update both.
        IReadOnlyList<ICommand> commands = [
            // A command to change the file type.
            new PropertyChangeCommand<ModelOutput, string>(
                DataSource,
                DataSource.OutputFileType,
                fileType.Metadata.FileName,
                (m, v) => m.OutputFileType = v),
            // A command to change the x-axis column.
            new PropertyChangeCommand<ModelOutput, string>(
                DataSource,
                DataSource.XAxisColumn,
                xcol,
                (m, v) => m.XAxisColumn = v),
            // A command to change the y-axis column.
            new PropertyChangeCommand<ModelOutput, string>(
                DataSource,
                DataSource.YAxisColumn,
                ycol,
                (m, v) => m.YAxisColumn = v)
        ];

        ICommand command = new CompositeCommand(commands);
        OnDataSourceChanged.Invoke(command);
    }

    /// <summary>
    /// Attempt to guess a good default x- and y- column name for the given
    /// output file type.
    /// </summary>
    /// <param name="fileType">The output file type.</param>
    /// <param name="xcol">The guessed x-column name.</param>
    /// <param name="ycol">The guessed y-column name.</param>
    private void GuessColumns(string fileType, out string xcol, out string ycol)
    {
        // TODO: we could improve the column selection by doing  partial-parse
        // of the output file type, but this is probably good enough for 90% of
        // cases.
        IEnumerable<string> columns = GetColumns(fileType);
        string[] toTry = ["Total", "total", "Mean", "mean"];
        foreach (string col in toTry)
        {
            if (columns.Contains(col))
            {
                xcol = "Date"; // Should be a safe guess?
                ycol = col;
                return;
            }
        }

        string[] toIgnore = ["Date", "Lon", "Lat", "patch", "stand", "indiv", "pft"];
        IEnumerable<string> remaining = columns.Except(toIgnore);
        if (remaining.Any())
        {
            xcol = "Date";
            ycol = remaining.First();
            return;
        }

        xcol = string.Empty;
        ycol = string.Empty;
    }
}
