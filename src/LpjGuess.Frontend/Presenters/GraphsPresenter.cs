using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Core.Models.Graphing.Style.Identifiers;
using LpjGuess.Core.Models.Graphing.Style.Providers;
using LpjGuess.Core.Models.Graphing.Style.Strategies;
using LpjGuess.Frontend.Factories;
using LpjGuess.Frontend.Interfaces.Factories;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which controls a graphs view to allow the user to navigate
/// between and view multiple graphs.
/// </summary>
public class GraphsPresenter : PresenterBase<IGraphsView>, IGraphsPresenter
{
	/// <summary>
	/// Dictionary mapping plot models to graph objects.
	/// </summary>
	private readonly Dictionary<IGraphView, IGraphPresenter> plots;

	/// <summary>
	/// The list of instruction files.
	/// </summary>
	private IEnumerable<string> instructionFiles;

	/// <summary>
	/// Factory for creating series presenters.
	/// </summary>
	private readonly ISeriesPresenterFactory seriesPresenterFactory;

	/// <summary>
	/// Create a new <see cref="GraphsPresenter"/> instance which displays the
	/// specified graphs on the specified view.
	/// </summary>
	/// <param name="view">The view object.</param>
	/// <param name="graphs">The graphs to be displayed.</param>
	/// <param name="instructionFiles">The instruction files for which data should be displayed.</param>
	public GraphsPresenter(
		IGraphsView view,
		IReadOnlyList<Graph> graphs,
		IEnumerable<string> instructionFiles) : base(view)
	{
		plots = new Dictionary<IGraphView, IGraphPresenter>();
		this.view.OnAddGraph.ConnectTo(OnAddGraph);
		this.view.OnRemoveGraph.ConnectTo(OnRemoveGraph);
		this.instructionFiles = instructionFiles;
		// TODO: consider dependency injection.
		seriesPresenterFactory = new SeriesPresenterFactory(
			new DataSourcePresenterFactory(instructionFiles));
		Populate(graphs);
	}

	/// <summary>
	/// Update the instruction files for which data should be displayed.
	/// </summary>
	/// <param name="instructionFiles">The instruction files for which data should be displayed.</param>
	public void UpdateInstructionFiles(IEnumerable<string> instructionFiles)
	{
		this.instructionFiles = instructionFiles;
		RefreshAll();
	}

	/// <inheritdoc />
	public void RefreshAll()
	{
		Populate(GetGraphs());
	}

	/// <inheritdoc />
    public IEnumerable<Graph> GetGraphs()
    {
        return plots.Values.Select(p => p.GetGraph());
    }

	/// <summary>
	/// Populate the view with the given list of plots.
	/// </summary>
	/// <param name="graphs"></param>
	private void Populate(IEnumerable<Graph> graphs)
	{
		plots.Values.ToList().ForEach(p => p.Dispose());
		plots.Clear();

		// Force evaluation, in case graphs depends on the plots collection.
		graphs = graphs.ToList();

		// Temporary list, to guarantee order.
		foreach (Graph graph in graphs)
		{
			// Construct new child presenter/view to display the oxyplot model.
			IGraphView graphView = new GraphView();
			IGraphPresenter presenter = new GraphPresenter(graphView, graph, instructionFiles, seriesPresenterFactory);
			presenter.OnTitleChanged.ConnectTo(t => OnGraphRenamed(t, graphView));

			plots.Add(graphView, presenter);
		}

		// This will remove any existing plots from the view.
		view.Populate(plots.Keys);
	}

	/// <summary>
	/// Called when the user wants to add a graph.
	/// </summary>
	private void OnAddGraph()
	{
		// TODO: reconsider how we initialise new graphs.
		// For now, this is enough.
		Graph graph = CreateDefaultGraph();

		List<Graph> graphs = plots.Values.Select(p => p.GetGraph()).ToList();
		graphs.Add(graph);

		// FIXME: this will redraw all existing plots. Would be better to just
		// add the new graph to the view.
		Populate(graphs);
	}

	/// <summary>
	/// Create a default graph.
	/// </summary>
	/// <remarks>
	/// TODO: rethink how this is handled. It's not really the responsibility of
	/// this presenter.
	/// </remarks>
	/// <returns>A graph object with sensible property values.</returns>
	private Graph CreateDefaultGraph()
	{
		IDataSource dataSource = new ModelOutput(
			"file_lai",
			"Date",
			["Total"],
			instructionFiles);
		ISeries series = new LineSeries(
			// Empty series name will use data source as title.
			string.Empty,
			new DynamicStyleProvider<Colour>(new GridcellIdentifier(), new ColourStrategy()),
			dataSource,
			AxisPosition.Bottom,
			AxisPosition.Left,
			new FixedStyleProvider<LineType>(LineType.Solid),
			new FixedStyleProvider<LineThickness>(LineThickness.Regular));
		return new Graph(string.Empty, [series]);
	}

	/// <summary>
	/// Called when a graph has been removed by the user.
	/// </summary>
	/// <param name="view">The graph that has been removed.</param>
	private void OnRemoveGraph(IGraphView view)
	{
		// Force immediate evaluation.
		IEnumerable<Graph> graphs = GetGraphs()
			.Except([plots[view].GetGraph()])
			.ToList();
		Populate(graphs);
	}

	/// <summary>
	/// Called after the user has renamed a graph.
	/// </summary>
	/// <param name="title">The new title.</param>
	/// <param name="view">The view associated with the graph.</param>
	private void OnGraphRenamed(string title, IGraphView view)
	{
		this.view.Rename(view, title);
	}
}
