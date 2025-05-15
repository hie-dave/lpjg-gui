using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which controls a graphs view to allow the user to navigate
/// between and view multiple graphs.
/// </summary>
public class GraphsPresenter : IGraphsPresenter
{
	/// <summary>
	/// The view object.
	/// </summary>
	private readonly IGraphsView view;

	/// <summary>
	/// Dictionary mapping plot models to graph objects.
	/// </summary>
	private readonly Dictionary<PlotModel, Graph> plots;

	/// <summary>
	/// Create a new <see cref="GraphsPresenter"/> instance which displays the
	/// specified graphs on the specified view.
	/// </summary>
	/// <param name="view">The view object.</param>
	/// <param name="graphs">The graphs to be displayed.</param>
	public GraphsPresenter(IGraphsView view, IReadOnlyList<Graph> graphs)
	{
		plots = new Dictionary<PlotModel, Graph>();
		this.view = view;
		this.view.OnAddGraph.ConnectTo(OnAddGraph);
		this.view.OnRemoveGraph.ConnectTo(OnRemoveGraph);
		Populate(graphs);
	}

    /// <inheritdoc />
    public void Dispose()
	{
		view.Dispose();
	}

	/// <summary>
	/// Populate the view with the given list of plots.
	/// </summary>
	/// <param name="graphs"></param>
	private void Populate(IEnumerable<Graph> graphs)
	{
		plots.Clear();

		// Temporary list, to guarantee order.
		List<PlotModel> models = new List<PlotModel>();
		foreach (Graph graph in graphs)
		{
			PlotModel model = ToOxyPlotModel(graph);
			models.Add(model);
			plots[model] = graph;
		}

		// This will remove any existing plots from the view.
		view.Populate(models);
	}

	/// <inheritdoc />
	public IGraphsView GetView() => view;

	/// <summary>
	/// Convert a graph object to an oxyplot plot model.
	/// </summary>
	/// <param name="graph">The graph object.</param>
	private PlotModel ToOxyPlotModel(Graph graph)
	{
		return OxyPlotConverter.ToPlotModel(graph);
	}

	// private void OnChartClick(object? sender, OxyMouseDownEventArgs e)
	// {
	// 	// if (e.HitTestResult != null && e.HitTestResult.Element is OxyPlot.Annotations.Annotation)
	// 	// 	plot1.TooltipText = (e.HitTestResult.Element as OxyPlot.Annotations.Annotation).ToolTip;
	// }

	/// <summary>
	/// Called when the user wants to add a graph.
	/// </summary>
	private void OnAddGraph()
	{
		Graph graph = new Graph("New Graph");
		List<Graph> graphs = plots.Values.ToList();
		graphs.Add(graph);
		Populate(graphs);
	}

    private void OnRemoveGraph(PlotModel model)
    {
		// Force immediate evaluation.
        IEnumerable<Graph> graphs = plots.Values.Except([plots[model]]).ToList();
		Populate(graphs);
    }

	/// <inheritdoc />
    public IEnumerable<Graph> GetGraphs()
    {
        return plots.Values;
    }
}
