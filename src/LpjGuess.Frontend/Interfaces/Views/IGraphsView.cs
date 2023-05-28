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
	/// Populate the graphs view with graphs.
	/// </summary>
	/// <param name="plots">The plots to be displayed.</param>
	void Populate(IReadOnlyList<PlotModel> plots);

	/// <summary>
	/// Add a single plot model.
	/// </summary>
	/// <param name="plot">The plot to be added.</param>
	void AddGraph(PlotModel plot);

	/// <summary>
	/// Get the plot models.
	/// </summary>
	IReadOnlyList<PlotModel> GetPlots();

	/// <summary>
	/// Select the specified graph.
	/// </summary>
	/// <param name="n">Index of the graph to be selected/displayed.</param>
	void SelectGraph(int n);
}
