using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Core.Models.Graphing.Style.Identifiers;
using LpjGuess.Core.Models.Graphing.Style.Providers;
using LpjGuess.Core.Models.Graphing.Style.Strategies;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which controls a graphs view to allow the user to navigate
/// between and view multiple graphs.
/// </summary>
[RegisterPresenter(typeof(IReadOnlyList<Graph>), typeof(IGraphsPresenter))]
public class GraphsPresenter : PresenterBase<IGraphsView, IReadOnlyList<Graph>>, IGraphsPresenter
{
	/// <summary>
	/// Dictionary mapping plot models to graph objects.
	/// </summary>
	private readonly Dictionary<IGraphView, IGraphPresenter> plots;

	/// <summary>
	/// Instruction files provider.
	/// </summary>
	private readonly IInstructionFilesProvider insFilesProvider;

	/// <summary>
	/// Factory for creating series presenters.
	/// </summary>
	private readonly IPresenterFactory presenterFactory;

	/// <summary>
	/// Create a new <see cref="GraphsPresenter"/> instance which displays the
	/// specified graphs on the specified view.
	/// </summary>
	/// <param name="view">The view object.</param>
	/// <param name="graphs">The graphs to be displayed.</param>
	/// <param name="instructionFiles">The instruction files for which data should be displayed.</param>
	/// <param name="presenterFactory">The presenter factory to use for creating series presenters.</param>
	/// <param name="registry">The command registry to use for command execution.</param>
	public GraphsPresenter(
		IGraphsView view,
		IReadOnlyList<Graph> graphs,
		IInstructionFilesProvider instructionFiles,
		IPresenterFactory presenterFactory,
		ICommandRegistry registry) : base(view, graphs, registry)
	{
		plots = new Dictionary<IGraphView, IGraphPresenter>();

		this.insFilesProvider = instructionFiles;
		this.presenterFactory = presenterFactory;

		this.view.OnAddGraph.ConnectTo(OnAddGraph);
		this.view.OnRemoveGraph.ConnectTo(OnRemoveGraph);
		instructionFiles.OnInstructionFilesChanged.ConnectTo(UpdateInstructionFiles);

		Populate(graphs);
	}

	/// <inheritdoc />
	public void RefreshAll()
	{
		Populate(GetGraphs());
	}

	/// <inheritdoc />
    public IReadOnlyList<Graph> GetGraphs()
    {
        return plots.Values.Select(p => p.GetGraph()).ToList();
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
			IGraphPresenter presenter = new GraphPresenter(graphView, graph, insFilesProvider.GetInstructionFiles(), presenterFactory, registry);
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
			insFilesProvider.GetInstructionFiles());
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

	/// <summary>
	/// Update the instruction files for which data should be displayed.
	/// </summary>
	/// <param name="instructionFiles">The instruction files for which data should be displayed.</param>
	private void UpdateInstructionFiles(IEnumerable<string> instructionFiles)
	{
		RefreshAll();
	}
}
