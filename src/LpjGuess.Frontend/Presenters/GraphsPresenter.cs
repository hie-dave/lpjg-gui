using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which controls a graphs view.
/// </summary>
public class GraphsPresenter : IGraphsPresenter
{
	/// <summary>
	/// The view object.
	/// </summary>
	private readonly IGraphsView view;

	/// <summary>
	/// Create a new <see cref="GraphsPresenter"/> instance which displays the
	/// specified graphs on the specified view.
	/// </summary>
	/// <param name="view">The view object.</param>
	/// <param name="graphs">The graphs to be displayed.</param>
	public GraphsPresenter(IGraphsView view, IReadOnlyList<Graph> graphs)
	{
		this.view = view;
		this.view.OnAddGraph.ConnectTo(OnAddGraph);
		view.Populate(graphs.Select(ToOxyPlotModel).ToList());
	}

	/// <inheritdoc />
	public void Dispose()
	{
		view.Dispose();
	}

	/// <inheritdoc />
	public IGraphsView GetView() => view;

	/// <inheritdoc />
	public IReadOnlyList<Graph> GetGraphs()
	{
		return view.GetPlots().Select(ToGraph).ToList();
	}

	/// <summary>
	/// Convert a graph object to an oxyplot plot model.
	/// </summary>
	/// <param name="graph">The graph object.</param>
	private PlotModel ToOxyPlotModel(Graph graph)
	{
		PlotModel plot = new PlotModel();
		plot.Title = graph.Title;
		foreach (ISeries series in graph.Series)
			plot.Series.Add(ToOxySeries(series));
		plot.Series.Add(SimpleLineSeries());
		plot.Axes.Add(new LinearAxis() { Position = AxisPosition.Left });
		plot.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom });
		plot.MouseDown += OnChartClick;
		return plot;
	}

	private void OnChartClick(object? sender, OxyMouseDownEventArgs e)
	{
		// if (e.HitTestResult != null && e.HitTestResult.Element is OxyPlot.Annotations.Annotation)
		// 	plot1.TooltipText = (e.HitTestResult.Element as OxyPlot.Annotations.Annotation).ToolTip;
	}

	private OxyPlot.Series.Series SimpleLineSeries()
	{
		LineSeries series = new LineSeries();
		series.Title = "Sample Series";
		series.Color = OxyColors.Blue;
		List<DataPoint> points = SimpleDataSeries();
		series.ToolTip = "this is a tooltip";
		series.ItemsSource = points;
		return series;
	}

	private List<DataPoint> SimpleDataSeries()
	{
		var points = new List<DataPoint>();
		double xmin = 0 * Math.PI;
		double xmax = 2 * Math.PI;
		Func<double, double> fx = x => Math.Sin(x);
		int n = 10_000;
		double x = xmin;
		double dlt = (xmax - xmin) / n;
		for (int i = 0; i < n; i++)
		{
			points.Add(new DataPoint(x, fx(x)));
			x += dlt;
		}
		return points;
	}

	/// <summary>
	/// Convert an lpjg series to an oxyplot series object.
	/// </summary>
	/// <param name="series">The series to be converted.</param>
	private OxyPlot.Series.Series ToOxySeries(ISeries series)
	{
		return SimpleLineSeries();
	}

	/// <summary>
	/// Convert an oxyplot plot model to a graph object.
	/// </summary>
	/// <param name="plot">The plot model.</param>
	private Graph ToGraph(PlotModel plot)
	{
		Graph graph = new Graph(plot.Title);
		foreach (var series in plot.Series)
			graph.Series.Add(ToSeries(series));
		return graph;
	}

	/// <summary>
	/// Convert an oxyplot series to an lpjg series object.
	/// </summary>
	/// <param name="series">The oxyplot series object.</param>
	private ISeries ToSeries(OxyPlot.Series.Series series)
	{
		return new LpjGuess.Core.Graphing.Series();
	}

	/// <summary>
	/// Called when the user wants to add a graph.
	/// </summary>
	private void OnAddGraph()
	{
		Graph graph = new Graph("New Graph");
		List<Graph> graphs = GetGraphs().ToList();
		graphs.Add(graph);
		// view.AddGraph(ToOxyPlotModel(graph));
		view.Populate(graphs.Select(ToOxyPlotModel));
	}
}
