using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Frontend.Delegates;
using OxyPlot;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view which displays a single graph.
/// </summary>
public interface IGraphView : IView
{
    /// <summary>
    /// Called when the user wants to add a series to the graph.
    /// </summary>
    Event OnAddSeries { get; }

    /// <summary>
    /// Called when the user wants to remove a series from the graph.
    /// </summary>
    Event<ISeries> OnRemoveSeries { get; }

    /// <summary>
    /// The plot model displayed in the view.
    /// </summary>
    PlotModel Model { get; }

    /// <summary>
    /// Update the plot model displayed in the view.
    /// </summary>
    /// <param name="model">The plot model to display.</param>
    void UpdatePlot(PlotModel model);

    /// <summary>
    /// Populate the list of series editor views.
    /// </summary>
    /// <param name="views">The series editor views to display.</param>
    void PopulateEditors(IEnumerable<(ISeries, ISeriesView)> views);
}
