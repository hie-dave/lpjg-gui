using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which controls a graph view to allow the user to
/// view and customize a single graph.
/// </summary>
public interface IGraphPresenter : IPresenter<IGraphView, Graph>
{
    /// <summary>
    /// Get the graph model managed by this presenter.
    /// </summary>
    /// <remarks>
    /// Is this actually needed?
    /// </remarks>
    /// <returns>The graph model.</returns>
    Graph GetGraph();

    /// <summary>
    /// Update the graph with the latest data.
    /// </summary>
    void RefreshData();

    /// <summary>
    /// Event raised when the graph title is changed.
    /// </summary>
    Event<string> OnTitleChanged { get; }
}
