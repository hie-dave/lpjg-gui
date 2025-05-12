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
	Event<PlotModel> OnRemoveGraph { get; }

	/// <summary>
	/// Populate the graphs view with graphs.
	/// </summary>
	/// <param name="plots">The plots to be displayed.</param>
	void Populate(IEnumerable<PlotModel> plots);
}
