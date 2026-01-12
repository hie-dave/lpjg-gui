using Gtk;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for editing a model output data source.
/// </summary>
public class ModelOutputView : IModelOutputView
{
    /// <summary>
    /// The view for selecting the output file type.
    /// </summary>
    private readonly OutputFilesDropDownView fileTypeView;

    /// <summary>
    /// The view for selecting the x-axis column.
    /// </summary>
    private readonly StringDropDownView xAxisColumnView;

    /// <summary>
    /// The view for selecting the y-axis column.
    /// </summary>
    private readonly ColumnSelectionView yAxisColumnView;

    /// <summary>
    /// The view for selecting the filters.
    /// </summary>
    private readonly ListBoxPopoverView filtersView;

    /// <summary>
    /// The lookup table for the filter views.
    /// </summary>
    private readonly IDictionary<IView, StyleVariationStrategy> strategyLookup;

    /// <inheritdoc/>
    public Event<IModelChange<ModelOutput>> OnEditDataSource { get; private init; }

    /// <inheritdoc/>
    public Event<OutputFile> OnFileTypeChanged { get; private init; }

    /// <inheritdoc/>
    public Event OnAddFilter => filtersView.OnAdd;

    /// <inheritdoc/>
    public Event<StyleVariationStrategy> OnRemoveFilter { get; private init; }

    /// <summary>
    /// Create a new <see cref="ModelOutputView"/> instance.
    /// </summary>
    /// <param name="factory">The logger factory.</param>
    public ModelOutputView(ILoggerFactory factory)
    {
        OnEditDataSource = new Event<IModelChange<ModelOutput>>();
        OnFileTypeChanged = new Event<OutputFile>();
        OnRemoveFilter = new Event<StyleVariationStrategy>();

        strategyLookup = new Dictionary<IView, StyleVariationStrategy>();

        fileTypeView = new OutputFilesDropDownView();
        fileTypeView.GetWidget().Hexpand = true;
        fileTypeView.OnDataItemSelected.ConnectTo(OnFileTypeChanged);

        xAxisColumnView = new StringDropDownView();
        xAxisColumnView.GetWidget().Hexpand = true;
        xAxisColumnView.OnSelectionChanged.ConnectTo(OnXAxisColumnChanged);

        yAxisColumnView = new ColumnSelectionView();
        yAxisColumnView.GetWidget().Hexpand = true;
        yAxisColumnView.OnSelectionChanged.ConnectTo(OnYAxisColumnChanged);

        filtersView = new ListBoxPopoverView(factory.CreateLogger<ListBoxPopoverView>());
        filtersView.AddText = "Add Filter";
        filtersView.OnRemove.ConnectTo(OnFilterRemoved);
    }

    /// <inheritdoc/>
    public IEnumerable<INamedView> GetGridConfigViews()
    {
        return [
            new NamedView(fileTypeView, "Output file"),
            new NamedView(xAxisColumnView, "X-axis column"),
            new NamedView(yAxisColumnView, "Y-axis columns")
        ];
    }

    /// <inheritdoc/>
    public IEnumerable<INamedView> GetExtraConfigViews()
    {
        return [
            new NamedView(filtersView, "Filters")
        ];
    }

    /// <inheritdoc/>
    public Widget GetWidget() => fileTypeView.GetWidget();

    /// <inheritdoc/>
    public void Dispose()
    {
        // Ownership of the wrapped widgets is passed to the series view.
    }

    /// <inheritdoc/>
    public void Populate(IEnumerable<OutputFile> fileTypes,
                         IEnumerable<string> xcols,
                         IEnumerable<string> ycols,
                         OutputFile fileType,
                         string xColumn,
                         IEnumerable<string> selectedColumns,
                         IEnumerable<IFilterView> filterViews)
    {
        fileTypeView.Populate(fileTypes);
        xAxisColumnView.Populate(xcols);
        yAxisColumnView.Populate(ycols);

        // This will fail if the collections don't contain the selected values.
        fileTypeView.Select(fileType);
        xAxisColumnView.Select(xColumn);
        yAxisColumnView.Select(selectedColumns);

        filtersView.Populate(filterViews
            .Select(v => new NamedView(v, Enum.GetName(v.Strategy)!.PascalToHumanCase())));
        strategyLookup.Clear();
        foreach (IFilterView view in filterViews)
            strategyLookup.Add(view, view.Strategy);
    }

    /// <summary>
    /// Called when the user removes a filter. Propagates the event back up to the
    /// owner of this view.
    /// </summary>
    /// <param name="view">The view of the filter to remove.</param>
    private void OnFilterRemoved(IView view)
    {
        if (!strategyLookup.TryGetValue(view, out StyleVariationStrategy strategy))
            throw new ArgumentException($"Style variation strategy corresponding to filter view not found in lookup table.");
        OnRemoveFilter.Invoke(strategy);
    }

    /// <summary>
    /// Called when the user changes the x-axis column. Propagates the event
    /// back up to the owner of this view.
    /// </summary>
    /// <param name="obj">The new x-axis column.</param>
    private void OnXAxisColumnChanged(string obj)
    {
        try
        {
            var change = new ModelChangeEventArgs<ModelOutput, string>(
                m => m.XAxisColumn,
                (m, v) => m.XAxisColumn = v,
                obj);
            OnEditDataSource.Invoke(change);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user changes the y-axis column. Propagates the event
    /// back up to the owner of this view.
    /// </summary>
    /// <param name="columns">The new y-axis columns.</param>
    private void OnYAxisColumnChanged(IEnumerable<string> columns)
    {
        try
        {
            var change = new ModelChangeEventArgs<ModelOutput, IEnumerable<string>>(
                m => m.YAxisColumns,
                (m, v) => m.YAxisColumns = v,
                columns);
            OnEditDataSource.Invoke(change);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
