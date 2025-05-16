using Dave.Benchmarks.Core.Services;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Factories;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility;
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

        // Connect event handlers
        view.OnAddSeries.ConnectTo(OnAddSeries);
        view.OnRemoveSeries.ConnectTo(OnRemoveSeries);

        // The plot model is initialised in RefreshData(), but the compiler
        // doesn't know this.
        plotModel = null!;
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
        // Create a new plot model
        plotModel = OxyPlotConverter.ToPlotModel(graph);

        // Update the view
        view.UpdatePlot(plotModel);

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
    /// Called when the series has been changed by the user.
    /// </summary>
    /// <param name="command">A command which will apply the change.</param>
    private void OnSeriesChanged(ICommand command)
    {
        command.Execute();
        RefreshData();
    }
}
