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
    /// Called when the user wants to change the title of the graph.
    /// </summary>
    Event<string> OnTitleChanged { get; }

    /// <summary>
    /// Called when the user wants to change the X-axis title of the graph.
    /// </summary>
    Event<string> OnXAxisTitleChanged { get; }

    /// <summary>
    /// Called when the user wants to change the Y-axis title of the graph.
    /// </summary>
    Event<string> OnYAxisTitleChanged { get; }

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
    /// Update the properties of the graph displayed in the view.
    /// </summary>
    /// <param name="title">The title of the graph.</param>
    /// <param name="xaxisTitle">The X-axis title of the graph.</param>
    /// <param name="yaxisTitle">The Y-axis title of the graph.</param>
    void UpdateProperties(string title, string? xaxisTitle, string? yaxisTitle);

    /// <summary>
    /// Populate the list of series editor views.
    /// </summary>
    /// <param name="views">The series editor views to display.</param>
    void PopulateEditors(IEnumerable<(ISeries, IView)> views);
}
