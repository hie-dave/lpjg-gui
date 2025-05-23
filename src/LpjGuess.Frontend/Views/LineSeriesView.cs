using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Frontend.Events;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// View for editing line series properties.
/// </summary>
public class LineSeriesView : SeriesViewBase<LineSeries>
{
    /// <summary>
    /// Dropdown for selecting the line type.
    /// </summary>
    private readonly StringDropDownView<LineType> lineTypeDropdown;

    /// <summary>
    /// Dropdown for selecting the line thickness.
    /// </summary>
    private readonly StringDropDownView<LineThickness> lineThicknessDropdown;

    /// <summary>
    /// Creates a new instance of LineSeriesView.
    /// </summary>
    public LineSeriesView() : base()
    {
        lineTypeDropdown = new EnumDropDownView<LineType>();
        AddControl("Line Type", lineTypeDropdown.GetWidget());

        lineThicknessDropdown = new EnumDropDownView<LineThickness>();
        AddControl("Line Thickness", lineThicknessDropdown.GetWidget());
    }

    /// <inheritdoc />
    protected override void PopulateView(LineSeries series)
    {
        DisconnectEvents();

        lineTypeDropdown.Select(series.Type);
        lineThicknessDropdown.Select(series.Thickness);

        ConnectEvents();
    }

    /// <inheritdoc />
    protected override void DisconnectEvents()
    {
        lineTypeDropdown.OnSelectionChanged.DisconnectAll();
        lineThicknessDropdown.OnSelectionChanged.DisconnectAll();
    }

    private void ConnectEvents()
    {
        lineTypeDropdown.OnSelectionChanged.ConnectTo(OnLineTypeChanged);
        lineThicknessDropdown.OnSelectionChanged.ConnectTo(OnLineThicknessChanged);
    }

    /// <summary>
    /// Called when the line thickness is changed.
    /// </summary>
    /// <param name="thickness">The new line thickness.</param>
    private void OnLineThicknessChanged(LineThickness thickness)
    {
        OnEditSeries.Invoke(new ModelChangeEventArgs<LineSeries, LineThickness>(
            series => series.Thickness,
            (series, value) => series.Thickness = value,
            thickness));
    }

    /// <summary>
    /// Called when the line type is changed.
    /// </summary>
    /// <param name="type">The new line type.</param>
    private void OnLineTypeChanged(LineType type)
    {
        OnEditSeries.Invoke(new ModelChangeEventArgs<LineSeries, LineType>(
            series => series.Type,
            (series, value) => series.Type = value,
            type));
    }
}
