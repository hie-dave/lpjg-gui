using Gtk;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility;

using Object = GObject.Object;
using NotifySignalArgs = GObject.Object.NotifySignalArgs;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// Base class for series views. This class is a box widget containing a grid.
/// The grid is populated with controls for editing properties common to all
/// series types. Any additional controls specific to a series type should be
/// added in the derived classes.
/// </summary>
public abstract class SeriesViewBase<T> : ViewBase<Box>, ISeriesView<T> where T : ISeries
{
    /// <summary>
    /// The property name of the rgba property on the colour button.
    /// </summary>
    private const string rgbaProperty = "rgba";

    /// <summary>
    /// The grid widget containing the controls.
    /// </summary>
    private readonly Grid container;

    /// <summary>
    /// Entry widget for editing the series title.
    /// </summary>
    private readonly Entry titleEntry;

    /// <summary>
    /// Button which opens a colour picker dialog.
    /// </summary>
    private readonly ColorDialogButton chooseColourButton;

    /// <summary>
    /// Dropdown for selecting the x-axis position.
    /// </summary>
    private readonly DropDownView<AxisPosition> xAxisPositionDropdown;

    /// <summary>
    /// Dropdown for selecting the y-axis position.
    /// </summary>
    private readonly DropDownView<AxisPosition> yAxisPositionDropdown;

    /// <summary>
    /// The number of rows of widgets currently in the grid.
    /// </summary>
    private int nrow = 0;

    /// <summary>
    /// Create a new <see cref="SeriesViewBase{T}"/> instance.
    /// </summary>
    protected SeriesViewBase() : base(new Box())
    {
        OnEditSeries = new Event<IModelChange<T>>();

        widget.SetOrientation(Orientation.Vertical);

        // Create container widget for controls.
        container = Grid.New();
        container.RowSpacing = 6;
        container.ColumnSpacing = 6;

        // Configure container with controls for editing common properties.
        titleEntry = Entry.New();
        AddControl("Title", titleEntry);

        ColorDialog colourDialog = new ColorDialog();
        colourDialog.Modal = true;

        chooseColourButton = ColorDialogButton.New(colourDialog);
        AddControl("Colour", chooseColourButton);

        // Axis position dropdowns should only display the valid axis positions
        // for their respective axis types.
        xAxisPositionDropdown = new DropDownView<AxisPosition>();
        xAxisPositionDropdown.Populate(
            [AxisPosition.Bottom, AxisPosition.Top],
            p => Enum.GetName(p)!);
        AddControl("X-axis position", xAxisPositionDropdown);

        yAxisPositionDropdown = new DropDownView<AxisPosition>();
        yAxisPositionDropdown.Populate(
            [AxisPosition.Left, AxisPosition.Right],
            p => Enum.GetName(p)!);
        AddControl("Y-axis position", yAxisPositionDropdown);

        // Pack children into this widget.
        widget.Append(container);

        // Connect events.
        ConnectBaseClassEvents();
    }

    /// <inheritdoc />
    public Event<IModelChange<T>> OnEditSeries { get; private init; }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnEditSeries.DisconnectAll();
        DisconnectBaseClassEvents();
        DisconnectEvents();
        base.Dispose();
    }

    /// <inheritdoc />
    public void Populate(T series)
    {
        DisconnectBaseClassEvents();

        titleEntry.SetText(series.Title);
        chooseColourButton.Rgba = ColourUtility.FromString(series.Colour);
        xAxisPositionDropdown.Select(series.XAxisPosition);
        yAxisPositionDropdown.Select(series.YAxisPosition);

        ConnectBaseClassEvents();

        PopulateView(series);
    }

    /// <summary>
    /// Add a control to the grid.
    /// </summary>
    /// <param name="name">The name of the control.</param>
    /// <param name="control">The control to add.</param>
    protected void AddControl(string name, Widget control)
    {
        Label label = Label.New(name);
        label.Halign = Align.Start;
        container.Attach(label, 0, nrow, 1, 1);
        container.Attach(control, 1, nrow, 1, 1);
        nrow++;
    }

    /// <summary>
    /// Populate the controls' current values with the values in the series.
    /// </summary>
    protected abstract void PopulateView(T series);

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    protected abstract void DisconnectEvents();

    /// <summary>
    /// Connect all base class events.
    /// </summary>
    private void ConnectBaseClassEvents()
    {
        titleEntry.OnActivate += OnTitleChanged;
        chooseColourButton.OnNotify += OnColourChanged;
        xAxisPositionDropdown.OnSelectionChanged.ConnectTo(OnXAxisPositionChanged);
        yAxisPositionDropdown.OnSelectionChanged.ConnectTo(OnYAxisPositionChanged);
    }

    /// <summary>
    /// Disconnect all base class events.
    /// </summary>
    private void DisconnectBaseClassEvents()
    {
        titleEntry.OnActivate -= OnTitleChanged;
        chooseColourButton.OnNotify -= OnColourChanged;
        xAxisPositionDropdown.OnSelectionChanged.DisconnectAll();
        yAxisPositionDropdown.OnSelectionChanged.DisconnectAll();
    }

    /// <summary>
    /// Called when the user has changed the title. This event is invoked when
    /// the entry widget is "activated". The keybindings for this signal are all
    /// forms of the enter key.
    /// </summary>
    /// <param name="sender">The entry widget.</param>
    /// <param name="args">Event data.</param>
    private void OnTitleChanged(Entry sender, EventArgs args)
    {
        try
        {
            // Get current title - don't have the action get a new title every
            // time it's invoked.
            string? title = titleEntry.GetText();
            OnEditSeries.Invoke(new ModelChangeEventArgs<T, string>(
                series => series.Title,
                (series, value) => series.Title = value,
                title));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user has changed the colour. This event is invoked when
    /// the colour button emits the "notify" signal.
    /// </summary>
    /// <param name="sender">The colour button.</param>
    /// <param name="args">Event data.</param>
    private void OnColourChanged(Object sender, NotifySignalArgs args)
    {
        try
        {
            string property = args.Pspec.GetName();
            if (property != rgbaProperty)
                return;
            Gdk.RGBA rgba = chooseColourButton.Rgba;
            OnEditSeries.Invoke(new ModelChangeEventArgs<T, string>(
                series => series.Colour,
                (series, value) => series.Colour = value,
                rgba.ToHex()));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the user has changed the y-axis position.
    /// </summary>
    /// <param name="position">The new y-axis position.</param>
    private void OnYAxisPositionChanged(AxisPosition position)
    {
        OnEditSeries.Invoke(new ModelChangeEventArgs<T, AxisPosition>(
            series => series.YAxisPosition,
            (series, value) => series.YAxisPosition = value,
            position));
    }

    /// <summary>
    /// Called when the user has changed the x-axis position.
    /// </summary>
    /// <param name="position">The new x-axis position.</param>
    private void OnXAxisPositionChanged(AxisPosition position)
    {
        OnEditSeries.Invoke(new ModelChangeEventArgs<T, AxisPosition>(
            series => series.XAxisPosition,
            (series, value) => series.XAxisPosition = value,
            position));
    }
}
