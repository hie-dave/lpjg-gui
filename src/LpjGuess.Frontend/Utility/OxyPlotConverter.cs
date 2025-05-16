using System.Data;
using Dave.Benchmarks.Core.Models;
using Dave.Benchmarks.Core.Models.Importer;
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
    /// <returns>An OxyPlot PlotModel.</returns>
    public static PlotModel ToPlotModel(Graph graph)
    {
        PlotModel plot = new PlotModel();
        plot.Title = graph.Title;

        // Add axes.
        foreach (OxyAxis axis in CreateAxes(graph))
            plot.Axes.Add(axis);

        // Add series.
        foreach (ISeries series in graph.Series)
            foreach (OxySeries oxySeries in ToOxySeries(series))
                plot.Series.Add(oxySeries);

        return plot;
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
            AxisPosition.Bottom => graph.XAxisTitle ?? GenerateAxisTitle(graph, requirements),
            AxisPosition.Left => graph.YAxisTitle ?? GenerateAxisTitle(graph, requirements),
            // FIXME: should add explicit properties for left/right/top/bottom axis titles.
            AxisPosition.Right => graph.YAxisTitle ?? GenerateAxisTitle(graph, requirements),
            AxisPosition.Top => graph.XAxisTitle ?? GenerateAxisTitle(graph, requirements),
            _ => throw new InvalidOperationException($"Unknown axis position: {position}")
        };
    }

    private static string GenerateAxisTitle(Graph graph, IGrouping<AxisPosition, AxisRequirements> requirements)
    {
        IEnumerable<string> titles = requirements.Select(r => r.Title);
        if (titles.Distinct().Count() == 1)
            return titles.First();
        return "Various Data";
    }

    private static OxyAxis CreateOxyAxis(IGrouping<AxisPosition, AxisRequirements> group)
    {
        AxisPosition position = group.Key;
        // Assuming AssertCompatibility has been called, all elements of this
        // group should have the same axis type.
        OxyAxis axis = CreateOxyAxis(group.First().Type);
        axis.Position = ToOxyAxisPosition(position);
        return axis;
    }

    private static OxyAxisPosition ToOxyAxisPosition(AxisPosition position)
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
    /// <returns>An OxyPlot Series.</returns>
    public static IEnumerable<OxySeries> ToOxySeries(ISeries series)
    {
        // FIXME - this probably doesn't work. Need to rethink the data provider API.
        IEnumerable<SeriesData> data = DataProviderFactory.Read(series.DataSource);

        return data.Select(seriesData => CreateOxySeries(series, seriesData));        
    }

    private static OxySeries CreateOxySeries(ISeries series, SeriesData data)
    {
        // Create the appropriate series type
        if (series is LineSeries line)
            return CreateLineSeries(line, data);
        // else if (series is ScatterSeries scatterSeries)
        //     scatterSeries2.MarkerFill = ColorUtility.HexToOxyColor(series.Colour);
        else
            throw new NotImplementedException($"Unsupported series type: {series.GetType().Name}");
    }

    private static OxyLineSeries CreateLineSeries(LineSeries series, SeriesData data)
    {
        OxyLineSeries lineSeries = new();
        lineSeries.Title = series.Title;
        lineSeries.Color = ColourUtility.HexToOxyColor(series.Colour);
        lineSeries.LineStyle = series.Type switch
        {
            LineType.Solid => LineStyle.Solid,
            LineType.Dashed => LineStyle.Dash,
            LineType.Dotted => LineStyle.Dot,
            _ => throw new InvalidOperationException($"Unknown line type: {series.Type}")
        };

        lineSeries.StrokeThickness = series.Thickness switch
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
