using Dave.Benchmarks.Core.Services;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identifiers;
using LpjGuess.Core.Models.Graphing.Style.Providers;
using LpjGuess.Core.Models.Graphing.Style.Strategies;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Factories;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility;
using LpjGuess.Frontend.Views;
using LpjGuess.Runner.Parsers;
using Microsoft.Extensions.Logging;
using OxyPlot;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which controls a graph view to allow the user to view and customize
/// a single graph.
/// </summary>
public class GraphPresenter : PresenterBase<IGraphView, Graph>, IGraphPresenter
{
    /// <summary>
    /// The graph model.
    /// </summary>
    private readonly Graph graph;

    /// <summary>
    /// Factory for creating series views.
    /// </summary>
    private readonly ISeriesPresenterFactory seriesPresenterFactory;

    /// <summary>
    /// The plot model.
    /// </summary>
    private PlotModel plotModel;

    /// <summary>
    /// The instruction file path.
    /// </summary>
    private IEnumerable<string> instructionFiles;

    /// <summary>
    /// List of presenters currently managing views for the graph's series.
    /// </summary>
    private List<ISeriesPresenter> seriesPresenters;

    /// <summary>
    /// The cancellation token source.
    /// </summary>
    private CancellationTokenSource cts;

    /// <summary>
    /// Event raised when the graph title is changed.
    /// </summary>
    public Event<string> OnTitleChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="GraphPresenter"/> instance.
    /// </summary>
    /// <param name="view">The view object.</param>
    /// <param name="graph">The graph model.</param>
    /// <param name="instructionFiles">The instruction files for which data should be displayed.</param>
    /// <param name="seriesPresenterFactory">Factory for creating series views.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    public GraphPresenter(
        IGraphView view,
        Graph graph,
        IEnumerable<string> instructionFiles,
        ISeriesPresenterFactory seriesPresenterFactory,
        ICommandRegistry registry)
        : base(view, graph, registry)
    {
        this.graph = graph;
        this.instructionFiles = instructionFiles;
        this.seriesPresenterFactory = seriesPresenterFactory;
        OnTitleChanged = new Event<string>();

        // Connect event handlers
        view.OnAddSeries.ConnectTo(OnAddSeries);
        view.OnRemoveSeries.ConnectTo(OnRemoveSeries);
        view.OnGraphChanged.ConnectTo(OnGraphChanged);

        // The plot model is initialised in RefreshData(), but the compiler
        // doesn't know this.
        plotModel = new();
        seriesPresenters = new();

        cts = new();
        RefreshData();
    }

    /// <inheritdoc />
    public Graph GetGraph() => graph;

    /// <inheritdoc />
    public void RefreshData()
    {
        CreatePlotModelAsync()
            .ContinueWithOnMainThread(OnPlotModelReady);
    }

    /// <summary>
    /// Called when the plot model is ready. This method is called on the main
    /// thread.
    /// </summary>
    /// <remarks>
    /// FIXME: This is a bandaid on the fact that we don't currently have true
    /// async support at the gtk signal handler level.
    /// </remarks>
    /// <param name="model">The plot model.</param>
    private void OnPlotModelReady(PlotModel model)
    {
        view.UpdatePlot(model);
        view.UpdateProperties(
            graph.Title,
            graph.XAxisTitle,
            graph.YAxisTitle,
            graph.Legend.Visible,
            graph.Legend.Position,
            graph.Legend.Placement,
            graph.Legend.Orientation,
            graph.Legend.BackgroundColour,
            graph.Legend.BorderColour);

        List<ISeriesPresenter> presenters = new();
        foreach (ISeries series in graph.Series)
        {
            ISeriesPresenter seriesPresenter = seriesPresenterFactory.CreatePresenter(series);
            seriesPresenter.OnSeriesChanged.ConnectTo(OnSeriesChanged);
            presenters.Add(seriesPresenter);
        }

        // This will remove the existing series editors.
        view.PopulateEditors(presenters.Select(p => (p.Series, p.GetView())));

        // Remove existing series presenters.
        seriesPresenters.ForEach(p => p.Dispose());
        seriesPresenters = presenters;

        if (model.Title != plotModel.Title)
            OnTitleChanged.Invoke(model.Title);

        plotModel = model;
    }

    /// <summary>
    /// Create the plot model for the graph.
    /// </summary>
    /// <returns>The plot model.</returns>
    private async Task<PlotModel> CreatePlotModelAsync()
    {
        try
        {
            // Cancel any existing plot conversion tasks.
            cts.Cancel();
            cts = new();
            return await OxyPlotConverter.ToPlotModelAsync(graph, cts.Token);
        }
        catch (Exception ex)
        {
            if (ex is not TaskCanceledException)
                MainView.RunOnMainThread(() => MainView.Instance.ReportError(ex));

            // If plot generation fails, return an empty plot model. This will
            // be displayed as a blank plot, which is the best we can do in this
            // case, but it also allows the user to continue editing the graph
            // properties to fix the error.
            return new PlotModel();
        }
    }

    /// <summary>
    /// Called when the user wants to add a series to the graph.
    /// </summary>
    private void OnAddSeries()
    {
        // TODO: ask user which type of series to add.
        // For now, create a model output series.
        LineSeries series = new LineSeries(
            string.Empty,
            new DynamicStyleProvider<Colour>(new GridcellIdentifier(), new ColourStrategy()),
            new ModelOutput("file_lai", "Date", ["Total"], instructionFiles),
            AxisPosition.Bottom,
            AxisPosition.Left,
            new FixedStyleProvider<LineType>(LineType.Solid),
            new FixedStyleProvider<LineThickness>(LineThickness.Regular));

        // Add the series to the graph
        graph.Series.Add(series);

        // Update the plot model
        RefreshData();
    }

    /// <summary>
    /// Called when the user wants to remove a series from the graph.
    /// </summary>
    /// <param name="series">The series to remove.</param>
    private void OnRemoveSeries(ISeries series)
    {
        // Remove the series from the graph
        graph.Series.Remove(series);
        
        // Update the plot model
        RefreshData();
    }

    /// <inheritdoc />
    public void UpdateInstructionFiles(IEnumerable<string> instructionFiles)
    {
        this.instructionFiles = instructionFiles;
        // TODO: update instruction files in any ModelOutputSeries??
    }

    /// <summary>
    /// Apply a change to the graph model and refresh the view.
    /// </summary>
    /// <param name="command"></param>
    protected override void InvokeCommand(ICommand command)
    {
        base.InvokeCommand(command);
        RefreshData();
    }

    /// <summary>
    /// Called when the graph has been changed by the user.
    /// </summary>
    /// <param name="change">The change to apply.</param>
    private void OnGraphChanged(IModelChange<Graph> change)
    {
        string title = graph.Title;
        ICommand command = change.ToCommand(graph);
        InvokeCommand(command);
        if (graph.Title != title)
            OnTitleChanged.Invoke(graph.Title);
    }

    /// <summary>
    /// Called when the series has been changed by the user.
    /// </summary>
    /// <param name="command">A command which will apply the change.</param>
    private void OnSeriesChanged(ICommand command)
    {
        InvokeCommand(command);
    }
}
