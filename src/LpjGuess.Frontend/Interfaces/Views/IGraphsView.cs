using LpjGuess.Frontend.Delegates;
using OxyPlot;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view which displays multiple graphs.
/// </summary>
public interface IGraphsView : IView
{
	/// <summary>
	/// Called when the user wants to add a graph.
	/// </summary>
	Event OnAddGraph { get; }

	/// <summary>
	/// Called when the user wants to remove a graph.
	/// </summary>
	Event<IGraphView> OnRemoveGraph { get; }

	/// <summary>
	/// Populate the graphs view with graphs.
	/// </summary>
	/// <param name="plots">The plots to be displayed.</param>
	void Populate(IEnumerable<IGraphView> plots);

	/// <summary>
	/// Rename a graph.
	/// </summary>
	/// <param name="view">The view associated with the graph.</param>
	/// <param name="title">The new title.</param>
	void Rename(IGraphView view, string title);
}
