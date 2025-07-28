using System.Data;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Importer;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Data;
using LpjGuess.Runner.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using DataPoint = OxyPlot.DataPoint;

// There are a lot of similar type names between this project and OxyPlot. The
// convention I'm following in this file is to prefix all oxyplot types with
// "Oxy". Any seemingly-ambiguous type name without such a prefix is therefore
// one of our types.
using OxySeries = OxyPlot.Series.Series;

using LineSeries = LpjGuess.Core.Models.Graphing.Series.LineSeries;
using OxyLineSeries = OxyPlot.Series.LineSeries;

using AxisPosition = LpjGuess.Core.Models.Graphing.AxisPosition;
using OxyAxisPosition = OxyPlot.Axes.AxisPosition;

using OxyAxis = OxyPlot.Axes.Axis;
using OxyLinearAxis = OxyPlot.Axes.LinearAxis;
using OxyDateTimeAxis = OxyPlot.Axes.DateTimeAxis;
using OxyLogarithmicAxis = OxyPlot.Axes.LogarithmicAxis;

using Legend = LpjGuess.Core.Models.Graphing.Legend;
using OxyLegend = OxyPlot.Legends.Legend;

using LegendPosition = LpjGuess.Core.Models.Graphing.LegendPosition;
using OxyLegendPosition = OxyPlot.Legends.LegendPosition;

using LegendPlacement = LpjGuess.Core.Models.Graphing.LegendPlacement;
using OxyLegendPlacement = OxyPlot.Legends.LegendPlacement;

using LegendOrientation = LpjGuess.Core.Models.Graphing.LegendOrientation;
using OxyLegendOrientation = OxyPlot.Legends.LegendOrientation;
using System.Threading;
using System.Diagnostics;

namespace LpjGuess.Frontend.Utility;

/// <summary>
/// Utility class for converting between LPJ-GUESS domain models and OxyPlot objects.
/// </summary>
public static class OxyPlotConverter
{
    /// <summary>
    /// Convert a Graph object to an OxyPlot PlotModel.
    /// </summary>
    /// <param name="graph">The graph to convert.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An OxyPlot PlotModel.</returns>
    public static async Task<PlotModel> ToPlotModelAsync(Graph graph, CancellationToken ct)
    {
        // Leaving this here for now, as it's easy to get in here too often, and
        // easy to fix.
        Console.WriteLine($"ToPlotModelAsync()");

        PlotModel plot = new PlotModel();
        plot.Title = GetPlotTitle(graph);

        // Default font size is very small for some reason.
        plot.DefaultFontSize *= 2;
        plot.TitleFontSize *= 2;
        plot.SubtitleFontSize *= 2;

        // Add axes.
        foreach (OxyAxis axis in CreateAxes(graph))
            plot.Axes.Add(axis);

        // Add series.
        int nseries = graph.Series.Sum(s => s.DataSource.GetNumSeries());
        StyleContext context = new StyleContext(nseries);
        foreach (ISeries series in graph.Series)
        {
            // Need to ensure the axis data is plotted on the correct axis,
            // because different series can have different axis positions.
            OxyAxis? xaxis = plot.Axes.FirstOrDefault(a => a.Position == series.XAxisPosition.ToOxyAxisPosition());
            OxyAxis? yaxis = plot.Axes.FirstOrDefault(a => a.Position == series.YAxisPosition.ToOxyAxisPosition());
            foreach (OxySeries oxySeries in await ToOxySeriesAsync(series, context, ct))
            {
                if (oxySeries is XYAxisSeries xySeries)
                {
                    if (xaxis != null)
                        xySeries.XAxisKey = xaxis.Key;
                    if (yaxis != null)
                        xySeries.YAxisKey = yaxis.Key;
                }
                plot.Series.Add(oxySeries);
            }
        }

        // Add legend.
        OxyLegend legend = new OxyLegend();
        legend.LegendOrientation = ToOxyOrientation(graph.Legend.Orientation);
        legend.LegendPlacement = ToOxyPlacement(graph.Legend.Placement);
        legend.LegendPosition = ToOxyPosition(graph.Legend.Position);
        legend.LegendBackground = graph.Legend.BackgroundColour.ToOxyColor();
        legend.LegendBorder = graph.Legend.BorderColour.ToOxyColor();
        legend.IsLegendVisible = graph.Legend.Visible;
        plot.Legends.Add(legend);

        return plot;
    }

    private static OxyLegendPosition ToOxyPosition(LegendPosition position)
    {
        return position switch
        {
            LegendPosition.TopLeft => OxyLegendPosition.TopLeft,
            LegendPosition.TopCenter => OxyLegendPosition.TopCenter,
            LegendPosition.TopRight => OxyLegendPosition.TopRight,
            LegendPosition.LeftTop => OxyLegendPosition.LeftTop,
            LegendPosition.LeftCenter => OxyLegendPosition.LeftMiddle,
            LegendPosition.LeftBottom => OxyLegendPosition.LeftBottom,
            LegendPosition.RightTop => OxyLegendPosition.RightTop,
            LegendPosition.RightCenter => OxyLegendPosition.RightMiddle,
            LegendPosition.RightBottom => OxyLegendPosition.RightBottom,
            LegendPosition.BottomLeft => OxyLegendPosition.BottomLeft,
            LegendPosition.BottomCenter => OxyLegendPosition.BottomCenter,
            LegendPosition.BottomRight => OxyLegendPosition.BottomRight,
            _ => throw new InvalidOperationException($"Unknown legend position: {position}")
        };
    }

    private static OxyLegendPlacement ToOxyPlacement(LegendPlacement placement)
    {
        return placement switch
        {
            LegendPlacement.Inside => OxyLegendPlacement.Inside,
            LegendPlacement.Outside => OxyLegendPlacement.Outside,
            _ => throw new InvalidOperationException($"Unknown legend placement: {placement}")
        };
    }

    private static OxyLegendOrientation ToOxyOrientation(LegendOrientation orientation)
    {
        return orientation switch
        {
            LegendOrientation.Vertical => OxyLegendOrientation.Vertical,
            LegendOrientation.Horizontal => OxyLegendOrientation.Horizontal,
            _ => throw new InvalidOperationException($"Unknown legend orientation: {orientation}")
        };
    }

    private static string GetPlotTitle(Graph graph)
    {
        if (!string.IsNullOrWhiteSpace(graph.Title))
            return graph.Title;

        // Fallback titles.
        if (graph.Series.Count == 1)
        {
            if (!string.IsNullOrWhiteSpace(graph.Series[0].Title))
                return graph.Series[0].Title;
            return graph.Series[0].DataSource.GetName();
        }

        return "";
    }

    /// <summary>
    /// Create the oxyplot axes required by a graph.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <returns>The oxyplot axes required by the graph.</returns>
    private static IEnumerable<OxyAxis> CreateAxes(Graph graph)
    {
        IEnumerable<AxisRequirements> axisRequirements = graph.GetAxisRequirements();
        AssertCompatibility(axisRequirements);

        // Our graph can only have at most one axis in each position.
        IEnumerable<IGrouping<AxisPosition, AxisRequirements>> groups = axisRequirements.GroupBy(axis => axis.Position);

        foreach (IGrouping<AxisPosition, AxisRequirements> group in groups)
        {
            OxyAxis axis = CreateOxyAxis(group);
            axis.Title = GetAxisTitle(graph, group);
            yield return axis;
        }
    }

    private static string GetAxisTitle(Graph graph, IGrouping<AxisPosition, AxisRequirements> requirements)
    {
        AxisPosition position = requirements.Key;
        return position switch
        {
            AxisPosition.Bottom => string.IsNullOrWhiteSpace(graph.XAxisTitle) ? GenerateAxisTitle(graph, requirements) : graph.XAxisTitle,
            AxisPosition.Left => string.IsNullOrWhiteSpace(graph.YAxisTitle) ? GenerateAxisTitle(graph, requirements) : graph.YAxisTitle,
            // FIXME: should add explicit properties for left/right/top/bottom axis titles.
            AxisPosition.Right => string.IsNullOrWhiteSpace(graph.YAxisTitle) ? GenerateAxisTitle(graph, requirements) : graph.YAxisTitle,
            AxisPosition.Top => string.IsNullOrWhiteSpace(graph.XAxisTitle) ? GenerateAxisTitle(graph, requirements) : graph.XAxisTitle,
            _ => throw new InvalidOperationException($"Unknown axis position: {position}")
        };
    }

    private static string GenerateAxisTitle(Graph graph, IGrouping<AxisPosition, AxisRequirements> requirements)
    {
        IEnumerable<string> titles = requirements.Select(r => r.Title);
        if (titles.Distinct().Count() != 1)
            return "Various Data";
        string title = titles.First();
        if (!string.IsNullOrWhiteSpace(title))
            return title;
        IEnumerable<string> dataNames = graph.Series.Select(s => DataProviderFactory.GetName(s.DataSource));
        if (dataNames.Distinct().Count() != 1)
            return graph.Title;
        return dataNames.First();
    }

    private static OxyAxis CreateOxyAxis(IGrouping<AxisPosition, AxisRequirements> group)
    {
        AxisPosition position = group.Key;
        // Assuming AssertCompatibility has been called, all elements of this
        // group should have the same axis type.
        OxyAxis axis = CreateOxyAxis(group.First().Type);
        axis.Position = ToOxyAxisPosition(position);
        axis.Key = Guid.NewGuid().ToString();
        return axis;
    }

    private static OxyAxisPosition ToOxyAxisPosition(this AxisPosition position)
    {
        return position switch
        {
            AxisPosition.Left => OxyAxisPosition.Left,
            AxisPosition.Bottom => OxyAxisPosition.Bottom,
            AxisPosition.Right => OxyAxisPosition.Right,
            AxisPosition.Top => OxyAxisPosition.Top,
            _ => throw new InvalidOperationException($"Unknown axis position: {position}")
        };
    }

    private static OxyAxis CreateOxyAxis(AxisType type)
    {
        return type switch
        {
            AxisType.Linear => new OxyLinearAxis(),
            AxisType.DateTime => new OxyDateTimeAxis(),
            AxisType.Logarithmic => new OxyLogarithmicAxis(),
            _ => throw new InvalidOperationException($"Unknown axis type: {type}")
        };
    }

    /// <summary>
    /// Ensure that all axis requirements are mutually compatible.
    /// </summary>
    /// <param name="axisRequirements">The axis requirements to check.</param>
    private static void AssertCompatibility(IEnumerable<AxisRequirements> axisRequirements)
    {
        // Assert compatibility between all combinations of axis requirements.
        foreach (AxisRequirements left in axisRequirements)
            foreach (AxisRequirements right in axisRequirements)
                left.AssertCompatibility(right);
    }

    /// <summary>
    /// Convert an ISeries to an OxyPlot Series.
    /// </summary>
    /// <param name="series">The series to convert.</param>
    /// <param name="context">The style context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An OxyPlot Series.</returns>
    public static async Task<IEnumerable<OxySeries>> ToOxySeriesAsync(ISeries series, StyleContext context, CancellationToken ct)
    {
        // FIXME - this probably doesn't work. Need to rethink the data provider API.
        IEnumerable<SeriesData> data = await DataProviderFactory.ReadAsync(series.DataSource, ct);

        return data.Select(seriesData => CreateOxySeries(series, seriesData, context));
    }

    private static OxySeries CreateOxySeries(ISeries series, SeriesData data, StyleContext context)
    {
        // Create the appropriate series type
        if (series is LineSeries line)
            return CreateLineSeries(line, data, context);
        // else if (series is ScatterSeries scatterSeries)
        //     scatterSeries2.MarkerFill = ColorUtility.HexToOxyColor(series.Colour);
        else
            throw new NotImplementedException($"Unsupported series type: {series.GetType().Name}");
    }

    private static OxyLineSeries CreateLineSeries(
        LineSeries series,
        SeriesData data,
        StyleContext context)
    {
        OxyLineSeries lineSeries = new();
        lineSeries.Title = series.Title;
        if (string.IsNullOrWhiteSpace(series.Title))
            lineSeries.Title = data.Name;

        lineSeries.Color = context.GetStyle(series.ColourProvider, data).ToOxyColor();
        lineSeries.LineStyle = context.GetStyle(series.Type, data) switch
        {
            LineType.Solid => LineStyle.Solid,
            LineType.Dashed => LineStyle.Dash,
            LineType.Dotted => LineStyle.Dot,
            _ => throw new InvalidOperationException($"Unknown line type: {series.Type}")
        };

        lineSeries.StrokeThickness = context.GetStyle(series.Thickness, data) switch
        {
            LineThickness.Thin => 1,
            LineThickness.Regular => 2,
            LineThickness.Thick => 4,
            _ => throw new InvalidOperationException($"Unknown line thickness: {series.Thickness}")
        };

        // Add data points if data is provided.
        lineSeries.ItemsSource = data.Data;

        return lineSeries;
    }
}
