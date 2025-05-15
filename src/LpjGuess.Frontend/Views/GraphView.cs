using Gtk;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;
using OxyPlot;
using OxyPlot.GtkSharp;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays a single graph and allows the user to customize it
/// with inline controls for immediate feedback.
/// </summary>
public class GraphView : Box, IGraphView
{
    /// <summary>
    /// Vertical spacing between elements in the editor box.
    /// </summary>
    private const int editorSpacing = 6;

    /// <summary>
    /// The plot view control for displaying the graph.
    /// </summary>
    private readonly PlotView plot;

    /// <summary>
    /// StackSidebar widget for managing series views.
    /// </summary>
    private readonly DynamicStackSidebar<ISeries> seriesSidebar;

    /// <summary>
    /// Revealer widget for managing the series sidebar.
    /// </summary>
    private readonly Revealer revealer;

    /// <inheritdoc />
    public Event OnAddSeries { get; private init; }

    /// <inheritdoc />
    public Event<ISeries> OnRemoveSeries { get; private init; }

    /// <summary>
    /// Create a new <see cref="GraphView"/> instance.
    /// </summary>
    public GraphView()
    {
        // Initialize events.
        OnAddSeries = new Event();
        OnRemoveSeries = new Event<ISeries>();

        // Configure plot view.
        plot = new PlotView();

        // Configure series sidebar.
        seriesSidebar = new DynamicStackSidebar<ISeries>(CreateSeriesSidebarWidget);
        seriesSidebar.OnAdd.ConnectTo(OnAddSeries);
        seriesSidebar.OnRemove.ConnectTo(OnRemoveSeries);

        Box editorBox = New(Orientation.Vertical, editorSpacing);
        editorBox.Append(seriesSidebar);

        // Create the revealer for the sidebar.
        revealer = Revealer.New();
        revealer.TransitionType = RevealerTransitionType.SlideRight;
        revealer.TransitionDuration = 250; // milliseconds
        revealer.RevealChild = false;
        revealer.SetChild(editorBox);

        // Create a header bar for the graph.
        HeaderBar header = HeaderBar.New();
        header.ShowTitleButtons = false;
        Button editButton = Button.NewFromIconName(Icons.Edit);
        editButton.OnClicked += OnEdit;
        header.PackEnd(editButton);

        Box contentBox = New(Orientation.Horizontal, 0);
        contentBox.Append(plot);
        contentBox.Append(revealer);

        // Configure the main container.
        SetOrientation(Orientation.Vertical);
        Append(header);
        Append(contentBox);
    }

    /// <inheritdoc />
    public Widget GetWidget() => this;

    /// <inheritdoc />
    public void UpdatePlot(PlotModel model)
    {
        // Apply dark theme if needed
        if (Settings.GetDefault()?.GtkApplicationPreferDarkTheme == true)
            model.TextColor = model.PlotAreaBorderColor = OxyColors.White;

        plot.Model = model;
    }

    /// <inheritdoc />
    public void PopulateEditors(IEnumerable<(ISeries, ISeriesView)> series)
    {
        seriesSidebar.Populate(series.Select(s => (s.Item1, s.Item2.GetWidget())));
    }

    /// <summary>
    /// Create a widget which will be displayed in the sidebar for the given series.
    /// </summary>
    /// <param name="series">The series to be displayed.</param>
    /// <returns>A label containing the series title.</returns>
    private Widget CreateSeriesSidebarWidget(ISeries series)
    {
        Label label = Label.New(series.Title);
        label.Halign = Align.Start;
        return label;
    }

    /// <summary>
    /// Called when the user has clicked the edit button.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnEdit(Button sender, EventArgs args)
    {
        try
        {
            revealer.RevealChild = !revealer.RevealChild;
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
