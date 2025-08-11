using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Entities;
using LpjGuess.Core.Models.Importer;
using LpjGuess.Core.Services;
using LpjGuess.Core.Interfaces;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Views.Dialogs;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Frontend.Data;
using LpjGuess.Core.Models.Graphing.Style.Identifiers;
using LpjGuess.Frontend.Extensions;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which controls a data source view to allow the user to view and
/// customize a single data source.
/// </summary>
[RegisterPresenter(typeof(ModelOutput), typeof(IDataSourcePresenter))]
public class ModelOutputPresenter : PresenterBase<IModelOutputView, ModelOutput>, IDataSourcePresenter<ModelOutput>
{
    /// <summary>
    /// The instruction files in the workspace.
    /// </summary>
    private readonly IInstructionFilesProvider instructionFilesProvider;

    /// <summary>
    /// The presenter factory to use for creating filter presenters.
    /// </summary>
    private readonly IPresenterFactory presenterFactory;

    /// <summary>
    /// The reader to use for reading data from the data source.
    /// </summary>
    private readonly ModelOutputReader reader;

    /// <summary>
    /// The filter presenters.
    /// </summary>
    private List<IFilterPresenter> filterPresenters;

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
    /// <param name="model">The data source being edited.</param>
    /// <param name="instructionFilesProvider">The instruction files provider.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    /// <param name="presenterFactory">The presenter factory to use for creating filter presenters.</param>
    /// <param name="reader">The reader to use for reading data from the data source.</param>
    public ModelOutputPresenter(
        IModelOutputView view,
        ModelOutput model,
        IInstructionFilesProvider instructionFilesProvider,
        ICommandRegistry registry,
        IPresenterFactory presenterFactory,
        ModelOutputReader reader) : base(view, model, registry)
    {
        filterPresenters = new List<IFilterPresenter>();
        DataSource = model;
        OnDataSourceChanged = new Event<ICommand>();

        this.instructionFilesProvider = instructionFilesProvider;
        this.presenterFactory = presenterFactory;
        this.reader = reader;

        view.OnEditDataSource.ConnectTo(OnEditDataSource);
        view.OnFileTypeChanged.ConnectTo(OnFileTypeChanged);
        view.OnAddFilter.ConnectTo(OnAddFilter);
        view.OnRemoveFilter.ConnectTo(OnRemoveFilter);

        RefreshView();
    }

    /// <summary>
    /// Refresh the data displayed in the view with that in the model object.
    /// </summary>
    public void RefreshView()
    {
        IEnumerable<OutputFile> fileTypes = instructionFilesProvider.GetGeneratedInstructionFiles()
            .Select(reader.GetSimulation)
            .SelectMany(s => s.GetOutputFiles())
            .DistinctBy(o => o.Metadata.FileName);

        OutputFile? outputFileType = fileTypes
            .FirstOrDefault(o => o.Metadata.FileName == DataSource.OutputFileType);
        OutputFileMetadata meta = outputFileType?.Metadata ?? OutputFileDefinitions.GetMetadata(DataSource.OutputFileType);

        if (outputFileType == null)
        {
            // TODO: think about exception handling (this can throw).
            outputFileType = new OutputFile(meta, string.Empty);
            if (!fileTypes.Any(f => f.Metadata.FileName == meta.FileName))
                fileTypes = fileTypes.Append(outputFileType);
        }

        IEnumerable<string> columns = GetColumns(DataSource.OutputFileType)
                .Append(DataSource.XAxisColumn)
                .Concat(DataSource.YAxisColumns)
                .Distinct();

        IEnumerable<string> ycols = columns
            .Where(meta.Layers.IsDataLayer)
            .Except(["Date"]);

        filterPresenters.ForEach(p => p.Dispose());
        filterPresenters.Clear();

        filterPresenters = model.Filters
            .Select(presenterFactory.CreatePresenter<IFilterPresenter>)
            .ToList();

        filterPresenters.ForEach(p => p.Populate(reader.GetIdentities(model, p.Model.Strategy)));
        filterPresenters.ForEach(p => p.OnFilterChanged.ConnectTo(OnDataSourceChanged));

        view.Populate(
            fileTypes,
            columns,
            ycols,
            outputFileType,
            DataSource.XAxisColumn,
            DataSource.YAxisColumns,
            filterPresenters.Select(p => p.GetView()));
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        filterPresenters.ForEach(p => p.Dispose());
        filterPresenters.Clear();
        base.Dispose();
    }

    /// <summary>
    /// Get a list of columns for the given output file type.
    /// </summary>
    /// <param name="fileType">The output file type.</param>
    /// <returns>A list of columns.</returns>
    private IEnumerable<string> GetColumns(string fileType)
    {
        IEnumerable<string> fileTypes = instructionFilesProvider.GetGeneratedInstructionFiles()
            .Select(reader.GetSimulation)
            .SelectMany(s => s.GetOutputFiles())
            .DistinctBy(o => o.Metadata.FileName)
            .Select(o => o.Metadata.FileName);

        List<string> columns = ["Date", "Lat", "Lon"];

        if (!fileTypes.Contains(fileType))
            // Unsure if this is possible.
            return columns;

        // Partially parse output file to get columns.
        // TODO: make this cancellable?
        IEnumerable<Task<IEnumerable<LayerMetadata>>> tasks = instructionFilesProvider.GetGeneratedInstructionFiles()
            .Select(reader.GetSimulation)
            .Select(s => s.ReadOutputFileMetadataAsync(fileType));
        Task.WaitAll(tasks);
        IEnumerable<LayerMetadata> metadata = tasks.SelectMany(t => t.Result).Distinct();

        // All  output files should have date, latitude, and longitude.
        columns.AddRange(metadata.Select(l => l.Name));

        // FIXME: patch-level outputs will not output a patch column if the
        // simulation contains only a single patch. It seemed like such a
        // clever idea at the time too...
        OutputFileMetadata meta = OutputFileDefinitions.GetMetadata(fileType);
        if (meta.Level > AggregationLevel.Gridcell)
            columns.Add("stand");
        if (meta.Level > AggregationLevel.Stand)
            columns.Add("patch");
        if (meta.Level > AggregationLevel.Patch)
            columns.Add("indiv");

        return columns.Distinct().ToList();
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

    /// <summary>
    /// Get the set of valid series contexts for the data source.
    /// </summary>
    /// <returns>The set of valid series contexts.</returns>
    private IEnumerable<SeriesContext> GetContexts()
    {
        // fixme - insanely inefficient
        Task<IEnumerable<SeriesData>> task = reader.ReadAsync(model, CancellationToken.None);
        task.Wait();
        return task.Result.Select(s => s.Context);
    }

    /// <summary>
    /// Get a list of allowed filter strategies.
    /// </summary>
    /// <returns>A list of allowed filter strategies.</returns>
    private IEnumerable<StyleVariationStrategy> GetAllowedFilterStrategies()
    {
        // Doesn't make sense to filter by "fixed"
        // Filtering by series is not yet implemented in an efficient way
        // Filtering by layer doesn't make sense because the model output works
        // by having the user explicitly choose which layers to plot.
        return model.GetAllowedStyleVariationStrategies()
            .Except([
                StyleVariationStrategy.Fixed,
                StyleVariationStrategy.BySeries,
                StyleVariationStrategy.ByLayer])
            .Except(model.Filters.OfType<DataFilter>().Select(f => f.Strategy))
            .ToList();
    }

    /// <summary>
    /// Get the filter with the given strategy.
    /// </summary>
    /// <param name="strategy">The strategy of the filter to get.</param>
    /// <returns>The filter with the given strategy.</returns>
    private IDataFilter GetFilter(StyleVariationStrategy strategy)
    {
        DataFilter? filter = model.Filters.OfType<DataFilter>().FirstOrDefault(f => f.Strategy == strategy);
        if (filter == null)
            throw new InvalidOperationException($"No filter with strategy {strategy} found.");
        return filter;
    }

    /// <summary>
    /// Get a description for the given filter strategy.
    /// </summary>
    /// <param name="strategy">The filter strategy.</param>
    /// <returns>A description for the filter strategy.</returns>
    private string GetFilterDescription(StyleVariationStrategy strategy)
    {
        return $"Filter {GetFilterName(strategy)}";
    }

    /// <summary>
    /// Get a name for the given filter strategy.
    /// </summary>
    /// <param name="strategy">The filter strategy.</param>
    /// <returns>A name for the filter strategy.</returns>
    private string GetFilterName(StyleVariationStrategy strategy)
    {
        return Enum.GetName(strategy)!.PascalToHumanCase();
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
            new PropertyChangeCommand<ModelOutput, IEnumerable<string>>(
                DataSource,
                DataSource.YAxisColumns,
                [ycol],
                (m, v) => m.YAxisColumns = v)
        ];

        ICommand command = new CompositeCommand(commands);
        OnDataSourceChanged.Invoke(command);
    }

    /// <summary>
    /// Called when the user wants to add a filter.
    /// </summary>
    private void OnAddFilter()
    {
        AskUserDialog.RunFor(
            GetAllowedFilterStrategies(),
            GetFilterName,
            GetFilterDescription,
            "Select a Filter Type",
            "Add",
            OnAddFilterOfType);
    }

    /// <summary>
    /// Called when the user wants to add a filter of the given strategy.
    /// </summary>
    /// <param name="strategy">The strategy to use for filtering.</param>
    private void OnAddFilterOfType(StyleVariationStrategy strategy)
    {
        AddElementCommand<IDataFilter> command = new(
            model.Filters,
            new DataFilter(strategy, []));
        OnDataSourceChanged.Invoke(command);
        RefreshView();
    }

    /// <summary>
    /// Called when the user wants to remove a filter.
    /// </summary>
    /// <param name="strategy">The strategy of the filter to remove.</param>
    private void OnRemoveFilter(StyleVariationStrategy strategy)
    {
        IDataFilter filter = GetFilter(strategy);
        RemoveElementCommand<IDataFilter> command = new(model.Filters, filter);
        OnDataSourceChanged.Invoke(command);
        RefreshView();
    }
}
