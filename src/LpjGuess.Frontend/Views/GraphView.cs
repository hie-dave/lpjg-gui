using Gtk;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility;
using LpjGuess.Frontend.Utility.Gtk;
using OxyPlot;
using OxyPlot.GtkSharp;

// Disambiguate IView from the OxyPlot interface with the same name.
using IView = LpjGuess.Frontend.Interfaces.IView;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays a single graph and allows the user to customize it
/// with inline controls for immediate feedback.
/// </summary>
public class GraphView : ViewBase<Box>, IGraphView
{
    /// <summary>
    /// Spacing between elements in the graph properties grid.
    /// </summary>
    private const int propertySpacing = 6;

    /// <summary>
    /// Vertical spacing between elements in the editor box.
    /// </summary>
    private const int editorSpacing = 6;

    /// <summary>
    /// Spacing within the frame widgets.
    /// </summary>
    private const int frameSpacing = 6;

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

    /// <summary>
    /// Entry widget for editing the graph title.
    /// </summary>
    private readonly Entry titleEntry;

    /// <summary>
    /// Entry widget for editing the X-axis title.
    /// </summary>
    private readonly Entry xaxisTitleEntry;

    /// <summary>
    /// Entry widget for editing the Y-axis title.
    /// </summary>
    private readonly Entry yaxisTitleEntry;

    /// <summary>
    /// Dropdown widget for selecting the legend position.
    /// </summary>
    private readonly EnumDropDownView<LegendPosition> legendPositionDropdown;

    /// <summary>
    /// Dropdown widget for selecting the legend placement.
    /// </summary>
    private readonly EnumDropDownView<LegendPlacement> legendPlacementDropdown;

    /// <summary>
    /// Dropdown widget for selecting the legend orientation.
    /// </summary>
    private readonly EnumDropDownView<LegendOrientation> legendOrientationDropdown;

    /// <summary>
    /// Button for selecting the legend background colour.
    /// </summary>
    private readonly ColourChangeView legendBackgroundButton;

    /// <summary>
    /// Button for selecting the legend border colour.
    /// </summary>
    private readonly ColourChangeView legendBorderButton;

    /// <summary>
    /// The grid container for graph properties.
    /// </summary>
    private readonly Grid graphProperties;

    /// <summary>
    /// Number of rows in the grid.
    /// </summary>
    private int nrow;

    /// <inheritdoc />
    public Event OnAddSeries { get; private init; }

    /// <inheritdoc />
    public Event<ISeries> OnRemoveSeries { get; private init; }

    /// <inheritdoc />
    public Event<IModelChange<Graph>> OnGraphChanged { get; private init; }

    /// <inheritdoc />
    public PlotModel Model => plot.Model;

    /// <summary>
    /// Create a new <see cref="GraphView"/> instance.
    /// </summary>
    public GraphView() : base(new Box())
    {
        // Initialize events.
        OnAddSeries = new Event();
        OnRemoveSeries = new Event<ISeries>();
        OnGraphChanged = new Event<IModelChange<Graph>>();

        // Configure plot view.
        plot = new PlotView();
        plot.Model = new();
        plot.Hexpand = true;

        // Configure series sidebar.
        seriesSidebar = new DynamicStackSidebar<ISeries>(CreateSeriesSidebarWidget);
        // Prevent the stack from expanding horizontally, so that the graph
        // takes up as much space as possible.
        seriesSidebar.StackHexpand = false;
        seriesSidebar.OnAdd.ConnectTo(OnAddSeries);
        seriesSidebar.OnRemove.ConnectTo(OnRemoveSeries);
        seriesSidebar.AddText = "Add Series";

        Frame seriesPropertiesFrame = new Frame();
        seriesPropertiesFrame.Child = seriesSidebar;
        seriesPropertiesFrame.Label = "Series Properties";
        seriesPropertiesFrame.LabelXalign = 0.5f;

        titleEntry = Entry.New();
        titleEntry.SetText(string.Empty);
        titleEntry.OnActivate += OnTitleEdited;
        titleEntry.Hexpand = true;

        xaxisTitleEntry = Entry.New();
        xaxisTitleEntry.SetText(string.Empty);
        xaxisTitleEntry.OnActivate += OnXAxisTitleEdited;
        xaxisTitleEntry.Hexpand = true;

        yaxisTitleEntry = Entry.New();
        yaxisTitleEntry.SetText(string.Empty);
        yaxisTitleEntry.OnActivate += OnYAxisTitleEdited;
        yaxisTitleEntry.Hexpand = true;

        legendPositionDropdown = new EnumDropDownView<LegendPosition>();
        legendPositionDropdown.OnSelectionChanged.ConnectTo(OnLegendPositionChanged);

        legendPlacementDropdown = new EnumDropDownView<LegendPlacement>();
        legendPlacementDropdown.OnSelectionChanged.ConnectTo(OnLegendPlacementChanged);

        legendOrientationDropdown = new EnumDropDownView<LegendOrientation>();
        legendOrientationDropdown.OnSelectionChanged.ConnectTo(OnLegendOrientationChanged);

        legendBackgroundButton = new ColourChangeView();
        legendBackgroundButton.OnChanged.ConnectTo(OnLegendBackgroundColourChanged);

        legendBorderButton = new ColourChangeView();
        legendBorderButton.OnChanged.ConnectTo(OnLegendBorderColourChanged);

        // A grid container for graph properties.
        graphProperties = new Grid();
        graphProperties.RowSpacing = propertySpacing;
        graphProperties.ColumnSpacing = propertySpacing;
        graphProperties.RowHomogeneous = true;
        graphProperties.MarginBottom = frameSpacing;
        graphProperties.MarginTop = frameSpacing;
        graphProperties.MarginStart = frameSpacing;
        graphProperties.MarginEnd = frameSpacing;

        AddRow("Title", titleEntry);
        AddRow("X-axis title", xaxisTitleEntry);
        AddRow("Y-axis title", yaxisTitleEntry);
        AddRow("Legend position", legendPositionDropdown.GetWidget());
        AddRow("Legend placement", legendPlacementDropdown.GetWidget());
        AddRow("Legend orientation", legendOrientationDropdown.GetWidget());
        AddRow("Legend background", legendBackgroundButton.GetWidget());
        AddRow("Legend border", legendBorderButton.GetWidget());

        Frame graphPropertiesFrame = new Frame();
        graphPropertiesFrame.Child = graphProperties;
        graphPropertiesFrame.Label = "Graph Properties";
        graphPropertiesFrame.LabelXalign = 0.5f;

        Box editorBox = Box.New(Orientation.Vertical, editorSpacing);
        editorBox.Append(graphPropertiesFrame);
        editorBox.Append(seriesPropertiesFrame);

        // Create the revealer for the sidebar.
        revealer = Revealer.New();
        revealer.TransitionType = RevealerTransitionType.SlideRight;
        revealer.TransitionDuration = 250; // milliseconds
        revealer.RevealChild = false;
        revealer.Hexpand = false;
        revealer.SetChild(editorBox);

        // Create a header bar for the graph.
        Box header = Box.New(Orientation.Horizontal, 0);
        header.Halign = Align.Start;
        ToggleButton editButton = new ToggleButton();
        editButton.TooltipText = "Show/hide graph configuration options";
        editButton.Active = false;
        editButton.IconName = Icons.Edit;
        editButton.OnToggled += OnEdit;
        header.Append(editButton);

        Box contentBox = Box.New(Orientation.Horizontal, 0);
        contentBox.Append(plot);
        contentBox.Append(revealer);

        // Configure the main container.
        widget.SetOrientation(Orientation.Vertical);
        widget.Append(header);
        widget.Append(contentBox);
    }

    /// <inheritdoc />
    public void UpdatePlot(PlotModel model)
    {
        // Apply dark theme if needed
        if (Settings.GetDefault()?.GtkApplicationPreferDarkTheme == true)
            model.TextColor = model.PlotAreaBorderColor = OxyColors.White;

        plot.Model = model;
    }

    /// <inheritdoc />
    public void UpdateProperties(
        string title,
        string? xaxisTitle,
        string? yaxisTitle,
        LegendPosition position,
        LegendPlacement placement,
        LegendOrientation orientation,
        Colour legendBackground,
        Colour legendBorder)
    {
        titleEntry.SetText(title);
        xaxisTitleEntry.SetText(xaxisTitle ?? string.Empty);
        yaxisTitleEntry.SetText(yaxisTitle ?? string.Empty);
        legendPositionDropdown.Select(position);
        legendPlacementDropdown.Select(placement);
        legendOrientationDropdown.Select(orientation);
        legendBackgroundButton.Populate(legendBackground);
        legendBorderButton.Populate(legendBorder);
    }

    /// <inheritdoc />
    public void PopulateEditors(IEnumerable<(ISeries, IView)> series)
    {
        seriesSidebar.Populate(series.Select(s => (s.Item1, s.Item2.GetWidget())));
    }

    /// <summary>
    /// Add a row to the graph properties grid.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="widget">The widget to display for the property.</param>
    private void AddRow(string name, Widget widget)
    {
        Label title = Label.New($"{name}:");
        title.Halign = Align.Start;
        graphProperties.Attach(title, 0, nrow, 1, 1);
        graphProperties.Attach(widget, 1, nrow, 1, 1);
        nrow++;
    }

    /// <summary>
    /// Create a widget which will be displayed in the sidebar for the given series.
    /// </summary>
    /// <param name="series">The series to be displayed.</param>
    /// <returns>A label containing the series title.</returns>
    private Widget CreateSeriesSidebarWidget(ISeries series)
    {
        string name = series.Title;
        if (string.IsNullOrWhiteSpace(name))
            name = "Untitled Series";
        Label label = Label.New(name);
        label.Halign = Align.Start;
        label.Hexpand = true;
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
            bool newState = !revealer.RevealChild;
            revealer.RevealChild = newState;
            sender.IconName = newState ? Icons.Checkmark : Icons.Edit;
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user has edited the title. This responds to the
    /// "activated" signal of the entry widget, which is emitted when the
    /// user hits the enter key.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnTitleEdited(Entry sender, EventArgs args)
    {
        try
        {
            OnGraphChanged.Invoke(
                new ModelChangeEventArgs<Graph, string>(
                    graph => graph.Title,
                    (graph, title) => graph.Title = title,
                    sender.GetText()
                )
            );
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user has edited the X-axis title. This responds to the
    /// "activated" signal of the entry widget, which is emitted when the
    /// user hits the enter key.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnXAxisTitleEdited(Entry sender, EventArgs args)
    {
        try
        {
            OnGraphChanged.Invoke(
                new ModelChangeEventArgs<Graph, string?>(
                    graph => graph.XAxisTitle,
                    (graph, title) => graph.XAxisTitle = title,
                    sender.GetText()
                )
            );
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user has edited the Y-axis title. This responds to the
    /// "activated" signal of the entry widget, which is emitted when the
    /// user hits the enter key.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnYAxisTitleEdited(Entry sender, EventArgs args)
    {
        try
        {
            OnGraphChanged.Invoke(
                new ModelChangeEventArgs<Graph, string?>(
                    graph => graph.YAxisTitle,
                    (graph, title) => graph.YAxisTitle = title,
                    sender.GetText()
                )
            );
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user changes the legend border colour.
    /// </summary>
    /// <param name="colour">The new border colour.</param>
    private void OnLegendBorderColourChanged(Colour colour)
    {
        OnGraphChanged.Invoke(
            new ModelChangeEventArgs<Graph, Colour>(
                graph => graph.Legend.BorderColour,
                (graph, colour) => graph.Legend.BorderColour = colour,
                colour
            )
        );
    }

    /// <summary>
    /// Called when the user changes the legend background colour.
    /// </summary>
    /// <param name="colour">The new background colour.</param>
    private void OnLegendBackgroundColourChanged(Colour colour)
    {
        OnGraphChanged.Invoke(
            new ModelChangeEventArgs<Graph, Colour>(
                graph => graph.Legend.BackgroundColour,
                (graph, colour) => graph.Legend.BackgroundColour = colour,
                colour
            )
        );
    }

    /// <summary>
    /// Called when the user changes the legend orientation.
    /// </summary>
    /// <param name="orientation">The new orientation.</param>
    private void OnLegendOrientationChanged(LegendOrientation orientation)
    {
        OnGraphChanged.Invoke(
            new ModelChangeEventArgs<Graph, LegendOrientation>(
                graph => graph.Legend.Orientation,
                (graph, orientation) => graph.Legend.Orientation = orientation,
                orientation
            )
        );
    }

    /// <summary>
    /// Called when the user changes the legend placement.
    /// </summary>
    /// <param name="placement">The new placement.</param>
    private void OnLegendPlacementChanged(LegendPlacement placement)
    {
        OnGraphChanged.Invoke(
            new ModelChangeEventArgs<Graph, LegendPlacement>(
                graph => graph.Legend.Placement,
                (graph, placement) => graph.Legend.Placement = placement,
                placement
            )
        );
    }

    /// <summary>
    /// Called when the user changes the legend position.
    /// </summary>
    /// <param name="position">The new position.</param>
    private void OnLegendPositionChanged(LegendPosition position)
    {
        OnGraphChanged.Invoke(
            new ModelChangeEventArgs<Graph, LegendPosition>(
                graph => graph.Legend.Position,
                (graph, position) => graph.Legend.Position = position,
                position
            )
        );
    }
}
