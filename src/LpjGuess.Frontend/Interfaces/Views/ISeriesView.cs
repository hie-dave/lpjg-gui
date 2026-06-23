using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view which allows the user to edit a single series.
/// </summary>
public interface ISeriesView<T> : IView where T : ISeries
{
    /// <summary>
    /// Called when the user wants to edit the series.
    /// The event parameter is the action to perform on the series.
    /// </summary>
    Event<IModelChange<T>> OnEditSeries { get; }

    /// <summary>
    /// Called when the user wants to change the data source type.
    /// </summary>
    Event<DataSourceType> OnDataSourceTypeChanged { get; }

    /// <summary>
    /// Populate the view.
    /// </summary>
    /// <param name="series">The series to populate the view with.</param>
    void Populate(T series);

    /// <summary>
    /// Show the data source views used by the series.
    /// </summary>
    /// <param name="yView">The y-axis data source view.</param>
    /// <param name="xView">The independent x-axis data source view, if enabled.</param>
    void ShowDataSourceViews(IDataSourceView yView, IDataSourceView? xView);

    /// <summary>
    /// Set the allowed style variation strategies.
    /// </summary>
    /// <param name="strategies">The allowed strategies.</param>
    void SetAllowedStyleVariationStrategies(IEnumerable<StyleVariationStrategy> strategies);
}
