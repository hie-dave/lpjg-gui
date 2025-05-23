using Dave.Benchmarks.Core.Services;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Commands;
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
public class GraphPresenter : IGraphPresenter
{
    /// <summary>
    /// The view object.
    /// </summary>
    private readonly IGraphView view;

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
    public GraphPresenter(IGraphView view, Graph graph, IEnumerable<string> instructionFiles, ISeriesPresenterFactory seriesPresenterFactory)
    {
        this.view = view;
        this.graph = graph;
        this.instructionFiles = instructionFiles;
        this.seriesPresenterFactory = seriesPresenterFactory;
        OnTitleChanged = new Event<string>();

        // Connect event handlers
        view.OnAddSeries.ConnectTo(OnAddSeries);
        view.OnRemoveSeries.ConnectTo(OnRemoveSeries);
        view.OnTitleChanged.ConnectTo(OnTitleEdited);
        view.OnXAxisTitleChanged.ConnectTo(OnXAxisTitleChanged);
        view.OnYAxisTitleChanged.ConnectTo(OnYAxisTitleChanged);

        // The plot model is initialised in RefreshData(), but the compiler
        // doesn't know this.
        plotModel = new();
        seriesPresenters = new();

        RefreshData();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        view.Dispose();
    }

    /// <inheritdoc />
    public IGraphView GetView() => view;

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
        view.UpdateProperties(graph.Title, graph.XAxisTitle, graph.YAxisTitle);

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
            return await OxyPlotConverter.ToPlotModelAsync(graph);
        }
        catch (Exception ex)
        {
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
        LineSeries series = new LineSeries
        {
            Title = "New Series",
            Thickness = LineThickness.Regular,
            Type = LineType.Solid,
            DataSource = new ModelOutput("file_lai", "Date", "Total", instructionFiles),
            Colour = "Blue"
        };

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
    private void ApplyChange(ICommand command)
    {
        command.Execute();
        RefreshData();
    }

    /// <summary>
    /// Called when the user has changed the graph title.
    /// </summary>
    /// <param name="title">The new title.</param>
    private void OnTitleEdited(string title)
    {
        // TODO: ensure stack sidebar in graphs view is updated.
        ICommand command = new PropertyChangeCommand<Graph, string>(
                graph, graph.Title, title, (g, t) => g.Title = t);
        ApplyChange(command);

        // Use plotModel.Title, rather than title as passed to this view. This
        // value is only used for display in the graphs sidebar (it's not
        // serialised). This only differs from the passed title if the title is
        // null or whitespace, in which case, a more useful name will be
        // generated by the oxyplot converter. In such a case, this generated
        // name will be more useful for the user than "Untitled Graph".
        OnTitleChanged.Invoke(plotModel.Title);
    }

    /// <summary>
    /// Called when the user has changed the X-axis title.
    /// </summary>
    /// <param name="title">The new title.</param>
    private void OnXAxisTitleChanged(string title)
    {
        ICommand command = new PropertyChangeCommand<Graph, string?>(
                graph, graph.XAxisTitle, title, (g, t) => g.XAxisTitle = t);
        ApplyChange(command);
    }

    /// <summary>
    /// Called when the user has changed the Y-axis title.
    /// </summary>
    /// <param name="title">The new title.</param>
    private void OnYAxisTitleChanged(string title)
    {
        ICommand command = new PropertyChangeCommand<Graph, string?>(
                graph, graph.YAxisTitle, title, (g, t) => g.YAxisTitle = t);
        ApplyChange(command);
    }

    /// <summary>
    /// Called when the series has been changed by the user.
    /// </summary>
    /// <param name="command">A command which will apply the change.</param>
    private void OnSeriesChanged(ICommand command)
    {
        command.Execute();
        RefreshData();
    }
}
