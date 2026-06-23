using Gtk;
using LpjGuess.Core.Interfaces;
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
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Core.Models;
using LpjGuess.Core.Extensions;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Core.Interfaces.Graphing.Style;
using System.ComponentModel;
using LpjGuess.Core.Models.Graphing.Style.Providers;
using LpjGuess.Frontend.Serialisation.Json;

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
    /// Name of the GtkSwitch active property.
    /// </summary>
    private const string activeProperty = "active";

    /// <summary>
    /// The grid widget containing the controls.
    /// </summary>
    private readonly Grid container;

    /// <summary>
    /// Entry widget for editing the series title.
    /// </summary>
    private readonly Entry titleEntry;

    /// <summary>
    /// Dropdown for selecting the colour strategy.
    /// </summary>
    private readonly EnumDropDownView<StyleVariationStrategy> colourStrategyDropdown;

    /// <summary>
    /// Button which opens a colour picker dialog.
    /// </summary>
    private readonly ColorDialogButton chooseColourButton;

    /// <summary>
    /// Dropdown for selecting the x-axis position.
    /// </summary>
    private readonly StringDropDownView<AxisPosition> xAxisPositionDropdown;

    /// <summary>
    /// Dropdown for selecting the y-axis position.
    /// </summary>
    private readonly StringDropDownView<AxisPosition> yAxisPositionDropdown;

    /// <summary>
    /// Dropdown for selecting the data source type.
    /// </summary>
    private readonly StringDropDownView<DataSourceType> dataSourceTypeDropdown;

    /// <summary>
    /// Switch controlling whether the series uses an independent x data source.
    /// </summary>
    private readonly Switch separateXDataSourceSwitch;

    /// <summary>
    /// The series currently displayed by the view.
    /// </summary>
    private T? series;

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
        OnDataSourceTypeChanged = new Event<DataSourceType>();

        widget.SetOrientation(Orientation.Vertical);
        widget.Spacing = 6;

        // Create container widget for controls.
        container = Grid.New();
        container.RowSpacing = 6;
        container.ColumnSpacing = 6;

        // Configure container with controls for editing common properties.
        titleEntry = Entry.New();
        AddControl("Title", titleEntry);

        ColorDialog colourDialog = new ColorDialog();
        colourDialog.Modal = true;

        colourStrategyDropdown = new EnumDropDownView<StyleVariationStrategy>();

        chooseColourButton = ColorDialogButton.New(colourDialog);
        AddControl("Colour", colourStrategyDropdown.GetWidget(), chooseColourButton);

        // Axis position dropdowns should only display the valid axis positions
        // for their respective axis types.
        xAxisPositionDropdown = new StringDropDownView<AxisPosition>(Enum.GetName!);
        xAxisPositionDropdown.Populate([AxisPosition.Bottom, AxisPosition.Top]);
        AddControl("X-axis position", xAxisPositionDropdown.GetWidget());

        yAxisPositionDropdown = new StringDropDownView<AxisPosition>(Enum.GetName!);
        yAxisPositionDropdown.Populate([AxisPosition.Left, AxisPosition.Right]);
        AddControl("Y-axis position", yAxisPositionDropdown.GetWidget());

        dataSourceTypeDropdown = new StringDropDownView<DataSourceType>(Enum.GetName!);
        dataSourceTypeDropdown.Populate(Enum.GetValues<DataSourceType>());

        separateXDataSourceSwitch = Switch.New();
        separateXDataSourceSwitch.Halign = Align.Start;
        separateXDataSourceSwitch.Valign = Align.Center;
        AddControl("Separate X data source", separateXDataSourceSwitch);

        // Pack children into this widget.
        widget.Append(container);

        // Connect events.
        ConnectBaseClassEvents();
    }

    /// <inheritdoc />
    public Event<IModelChange<T>> OnEditSeries { get; private init; }

    /// <inheritdoc />
    public Event<DataSourceType> OnDataSourceTypeChanged { get; private init; }

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
        this.series = series;
        DisconnectBaseClassEvents();

        titleEntry.SetText(series.Title);
        if (series.ColourProvider is FixedStyleProvider<Colour> fixedProvider)
        {
            chooseColourButton.Rgba = fixedProvider.Style.ToRgba();
            chooseColourButton.Show();
            container.SetColumnSpan(colourStrategyDropdown.GetWidget(), 1);
        }
        else
        {
            // Not using a fixed colour, so hide the colour button.
            chooseColourButton.Hide();
            container.SetColumnSpan(colourStrategyDropdown.GetWidget(), 2);
        }

        colourStrategyDropdown.Select(series.ColourProvider.GetStrategy());
        xAxisPositionDropdown.Select(series.XAxisPosition);
        yAxisPositionDropdown.Select(series.YAxisPosition);
        separateXDataSourceSwitch.Active = series.XDataSource is not null;
        separateXDataSourceSwitch.State = series.XDataSource is not null;

        ConnectBaseClassEvents();

        PopulateView(series);
    }

    /// <inheritdoc />
    public void ShowDataSourceViews(IDataSourceView yView, IDataSourceView? xView)
    {
        if (xView is not null)
            widget.Append(CreateDataSourceFrame("X data source", xView));
        widget.Append(CreateDataSourceFrame(
            xView is null ? "Data source" : "Y data source",
            yView));
    }

    /// <inheritdoc />
    public virtual void SetAllowedStyleVariationStrategies(IEnumerable<StyleVariationStrategy> strategies)
    {
        colourStrategyDropdown.Populate(strategies);
    }

    /// <summary>
    /// Set the column span of a widget.
    /// </summary>
    /// <param name="widget">The widget to set the column span of.</param>
    /// <param name="span">The column span.</param>
    protected void SetColumnSpan(Widget widget, int span)
    {
        container.SetColumnSpan(widget, span);
    }

    /// <summary>
    /// Add a control and optional value control (in the 3rd column) to the
    /// grid.
    /// </summary>
    /// <param name="name">The name of the control.</param>
    /// <param name="control">The control to add.</param>
    /// <param name="valueControl">The control to add to the right of the
    /// control.</param>
    protected void AddControl(string name, Widget control, Widget? valueControl = null)
    {
        Label label = Label.New($"{name}:");
        label.Halign = Align.Start;
        container.Attach(label, 0, nrow, 1, 1);
        container.Attach(control, 1, nrow, valueControl == null ? 2 : 1, 1);
        if (valueControl != null)
            container.Attach(valueControl, 2, nrow, 1, 1);
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
        colourStrategyDropdown.OnSelectionChanged.ConnectTo(OnColourStrategyChanged);
        separateXDataSourceSwitch.OnNotify += OnSeparateXDataSourceChanged;
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
        colourStrategyDropdown.OnSelectionChanged.DisconnectAll();
        separateXDataSourceSwitch.OnNotify -= OnSeparateXDataSourceChanged;
    }

    /// <summary>
    /// Build a frame containing all controls for a data source.
    /// </summary>
    private static Frame CreateDataSourceFrame(string title, IDataSourceView view)
    {
        Grid grid = Grid.New();
        grid.RowSpacing = 6;
        grid.ColumnSpacing = 6;
        int row = 0;

        foreach (INamedView named in view.GetGridConfigViews())
        {
            Label label = Label.New($"{named.Name}:");
            label.Halign = Align.Start;
            grid.Attach(label, 0, row, 1, 1);
            grid.Attach(named.View.GetWidget(), 1, row, 1, 1);
            row++;
        }

        Box contents = Box.New(Orientation.Vertical, 6);
        contents.Append(grid);
        foreach (INamedView named in view.GetExtraConfigViews())
        {
            Frame extra = Frame.New(named.Name);
            extra.SetChild(named.View.GetWidget());
            contents.Append(extra);
        }

        Frame frame = Frame.New(title);
        frame.SetChild(contents);
        return frame;
    }

    /// <summary>
    /// Enable or disable an independent x-axis data source.
    /// </summary>
    private void OnSeparateXDataSourceChanged(Object sender, NotifySignalArgs args)
    {
        if (args.Pspec.GetName() != activeProperty || series is null)
            return;

        IDataSource? newSource = separateXDataSourceSwitch.Active
            ? JsonSerialisation.DeepCloneRuntimeType(series.YDataSource)
            : null;
        OnEditSeries.Invoke(new ModelChangeEventArgs<T, IDataSource?>(
            s => s.XDataSource,
            (s, value) => s.XDataSource = value,
            newSource));
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

            // Capture value now, as it may change before the event is handled.
            Colour colour = chooseColourButton.Rgba.ToColour();
            OnEditSeries.Invoke(
                new ModelChangeEventArgs<T, IStyleProvider<Colour>>(
                    series => series.ColourProvider,
                    (series, provider) => series.ColourProvider = provider,
                    new FixedStyleProvider<Colour>(colour)
                )
            );
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

    /// <summary>
    /// Called when the user has changed the colour strategy.
    /// </summary>
    /// <param name="strategy">The new colour strategy.</param>
    private void OnColourStrategyChanged(StyleVariationStrategy strategy)
    {
        OnEditSeries.Invoke(new ColourProviderChangeEvent<T>(
            strategy,
            series => series.ColourProvider,
            (series, provider) => series.ColourProvider = provider));
    }
}
