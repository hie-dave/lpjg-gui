using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Utility;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// View for editing line series properties.
/// </summary>
public class LineSeriesView : SeriesViewBase<LineSeries>
{
    /// <summary>
    /// Dropdown for selecting the line type strategy.
    /// </summary>
    private readonly EnumDropDownView<StyleVariationStrategy> lineTypeStrategyDropdown;

    /// <summary>
    /// Dropdown for selecting the line type.
    /// </summary>
    private readonly StringDropDownView<LineType> lineTypeDropdown;

    /// <summary>
    /// Dropdown for selecting the line thickness strategy.
    /// </summary>
    private readonly EnumDropDownView<StyleVariationStrategy> lineThicknessStrategyDropdown;

    /// <summary>
    /// Dropdown for selecting the line thickness.
    /// </summary>
    private readonly StringDropDownView<LineThickness> lineThicknessDropdown;

    /// <summary>
    /// Creates a new instance of LineSeriesView.
    /// </summary>
    public LineSeriesView() : base()
    {
        lineTypeStrategyDropdown = new EnumDropDownView<StyleVariationStrategy>();
        lineTypeDropdown = new EnumDropDownView<LineType>();
        AddControl(
            "Line Type",
            lineTypeStrategyDropdown.GetWidget(),
            lineTypeDropdown.GetWidget());

        lineThicknessStrategyDropdown = new EnumDropDownView<StyleVariationStrategy>();
        lineThicknessDropdown = new EnumDropDownView<LineThickness>();
        AddControl(
            "Line Thickness",
            lineThicknessStrategyDropdown.GetWidget(),
            lineThicknessDropdown.GetWidget());
    }

    /// <inheritdoc />
    protected override void PopulateView(LineSeries series)
    {
        DisconnectEvents();

        if (series.Type is FixedStyleProvider<LineType> fixedLineType)
        {
            lineTypeDropdown.Select(fixedLineType.Style);
            lineTypeDropdown.GetWidget().Show();
        }
        else
            lineTypeDropdown.GetWidget().Hide();

        if (series.Thickness is FixedStyleProvider<LineThickness> thickness)
        {
            lineThicknessDropdown.Select(thickness.Style);
            lineThicknessDropdown.GetWidget().Show();
        }
        else
            lineThicknessDropdown.GetWidget().Hide();

        ConnectEvents();
    }

    /// <inheritdoc />
    protected override void DisconnectEvents()
    {
        lineTypeDropdown.OnSelectionChanged.DisconnectAll();
        lineThicknessDropdown.OnSelectionChanged.DisconnectAll();

        lineTypeStrategyDropdown.OnSelectionChanged.DisconnectAll();
        lineThicknessStrategyDropdown.OnSelectionChanged.DisconnectAll();
    }

    /// <summary>
    /// Connect event sources to sinks.
    /// </summary>
    private void ConnectEvents()
    {
        lineTypeDropdown.OnSelectionChanged.ConnectTo(OnLineTypeChanged);
        lineThicknessDropdown.OnSelectionChanged.ConnectTo(OnLineThicknessChanged);

        lineTypeStrategyDropdown.OnSelectionChanged.ConnectTo(OnLineTypeStrategyChanged);
        lineThicknessStrategyDropdown.OnSelectionChanged.ConnectTo(OnLineThicknessStrategyChanged);
    }

    /// <summary>
    /// Called when the line thickness strategy is changed.
    /// </summary>
    /// <param name="strategy">The new line thickness strategy.</param>
    private void OnLineThicknessStrategyChanged(StyleVariationStrategy strategy)
    {
        OnEditSeries.Invoke(new EnumProviderChangeEvent<LineSeries, LineThickness>(
            strategy,
            series => series.Thickness,
            (series, provider) => series.Thickness = provider));
    }

    /// <summary>
    /// Called when the line type strategy is changed.
    /// </summary>
    /// <param name="strategy">The new line type strategy.</param>
    private void OnLineTypeStrategyChanged(StyleVariationStrategy strategy)
    {
        OnEditSeries.Invoke(new EnumProviderChangeEvent<LineSeries, LineType>(
            strategy,
            series => series.Type,
            (series, provider) => series.Type = provider));
    }

    /// <summary>
    /// Called when the line thickness is changed.
    /// </summary>
    /// <param name="thickness">The new line thickness.</param>
    private void OnLineThicknessChanged(LineThickness thickness)
    {
        OnEditSeries.Invoke(new ModelChangeEventArgs<LineSeries, IStyleProvider<LineThickness>>(
            series => series.Thickness,
            (series, provider) => series.Thickness = provider,
            new FixedStyleProvider<LineThickness>(thickness)));
    }

    /// <summary>
    /// Called when the line type is changed.
    /// </summary>
    /// <param name="type">The new line type.</param>
    private void OnLineTypeChanged(LineType type)
    {
        OnEditSeries.Invoke(new ModelChangeEventArgs<LineSeries, IStyleProvider<LineType>>(
            series => series.Type,
            (series, provider) => series.Type = provider,
            new FixedStyleProvider<LineType>(type)));
    }
}
