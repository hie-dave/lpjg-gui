using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;
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
    /// Called when the user wants to change the graph.
    /// </summary>
    Event<IModelChange<Graph>> OnGraphChanged { get; }

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
    /// <param name="legendVisible">Whether the legend is visible.</param>
    /// <param name="position">The position of the legend.</param>
    /// <param name="placement">The placement of the legend.</param>
    /// <param name="orientation">The orientation of the legend.</param>
    /// <param name="legendBackground">The background colour of the legend.</param>
    /// <param name="legendBorder">The border colour of the legend.</param>
    void UpdateProperties(
        string title,
        string? xaxisTitle,
        string? yaxisTitle,
        bool legendVisible,
        LegendPosition position,
        LegendPlacement placement,
        LegendOrientation orientation,
        Colour legendBackground,
        Colour legendBorder);

    /// <summary>
    /// Populate the list of series editor views.
    /// </summary>
    /// <param name="views">The series editor views to display.</param>
    void PopulateEditors(IEnumerable<(ISeries, IView)> views);
}
