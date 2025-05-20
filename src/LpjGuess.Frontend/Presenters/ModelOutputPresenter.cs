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
        this.instructionFiles = instructionFiles;

        RefreshView();
    }

    /// <summary>
    /// Refresh the data displayed in the view with that in the model object.
    /// </summary>
    public void RefreshView()
    {
        IEnumerable<string> fileTypes = instructionFiles
            .Select(ModelOutputReader.GetSimulation)
            .SelectMany(s => s.GetOutputFiles())
            .DistinctBy(o => o.Metadata.FileName)
            .Select(o => o.Metadata.FileName);

        IEnumerable<string> columns;
        if (!fileTypes.Contains(DataSource.OutputFileType))
        {
            fileTypes = fileTypes.Append(DataSource.OutputFileType);
            columns = [DataSource.XAxisColumn, DataSource.YAxisColumn];
        }
        else
        {
            // Parse output file to get columns.
            IEnumerable<Task<IEnumerable<LayerMetadata>>> tasks = instructionFiles
                .Select(ModelOutputReader.GetSimulation)
                .Select(s => s.ReadOutputFileMetadataAsync(DataSource.OutputFileType));
            Task.WaitAll(tasks);
            IEnumerable<LayerMetadata> metadata = tasks.SelectMany(t => t.Result).Distinct();
            // All  output files should have date, latitude, and longitude.
            columns = ["Date", "Lat", "Lon"];
            columns = columns.Concat(metadata.Select(l => l.Name));

            // FIXME: patch-level outputs will not output a patch column if the
            // simulation contains only a single patch. It seemed like such a
            // clever idea at the time too...
            OutputFileMetadata meta = OutputFileDefinitions.GetMetadata(DataSource.OutputFileType);
            if (meta.Level  > AggregationLevel.Gridcell)
                columns = columns.Append("stand");
            if (meta.Level > AggregationLevel.Stand)
                columns = columns.Append("patch");
            if (meta.Level > AggregationLevel.Patch)
                columns = columns.Append("indiv");

            if (!columns.Contains(DataSource.XAxisColumn))
                columns = columns.Append(DataSource.XAxisColumn);
            if (!columns.Contains(DataSource.YAxisColumn))
                columns = columns.Append(DataSource.YAxisColumn);
        }

        view.Populate(
            fileTypes,
            columns,
            DataSource.OutputFileType,
            DataSource.XAxisColumn,
            DataSource.YAxisColumn);
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
}
