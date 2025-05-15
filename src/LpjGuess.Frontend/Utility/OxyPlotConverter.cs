using System.Data;
using Dave.Benchmarks.Core.Models;
using Dave.Benchmarks.Core.Models.Importer;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Runner.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using DataPoint = OxyPlot.DataPoint;

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
    /// <param name="data">Optional data table to use for data series.</param>
    /// <returns>An OxyPlot PlotModel.</returns>
    public static PlotModel ToPlotModel(Graph graph, DataTable? data = null)
    {
        PlotModel plot = new PlotModel();
        plot.Title = graph.Title;

        // Add default axes if none exist
        plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y Axis" });
        plot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X Axis" });
        
        // Add series
        foreach (ISeries series in graph.Series)
        {
            OxyPlot.Series.Series oxySeries = ToOxySeries(series, data);
            plot.Series.Add(oxySeries);
        }
        
        return plot;
    }
    
    /// <summary>
    /// Convert an ISeries to an OxyPlot Series.
    /// </summary>
    /// <param name="series">The series to convert.</param>
    /// <param name="data">Optional data table to use for data points.</param>
    /// <returns>An OxyPlot Series.</returns>
    public static OxyPlot.Series.Series ToOxySeries(ISeries series, DataTable? data = null)
    {
        OxyPlot.Series.Series oxySeries;
        
        // Create the appropriate series type
        if (series.Type == SeriesType.Line)
        {
            OxyPlot.Series.LineSeries lineSeries = new OxyPlot.Series.LineSeries();
            lineSeries.Title = series.Title;

            // Add data points if data is provided
            if (data != null && series is ModelOutputSeries modelSeries)
            {
                AddDataPoints(lineSeries, modelSeries, data);
            }

            oxySeries = lineSeries;
        }
        else // Scatter
        {
            ScatterSeries scatterSeries = new ScatterSeries { Title = series.Title };
            
            // Add data points if data is provided
            if (data != null && series is ModelOutputSeries modelSeries)
            {
                AddScatterPoints(scatterSeries, modelSeries, data);
            }
            
            oxySeries = scatterSeries;
        }
        
        // Set color
        if (oxySeries is OxyPlot.Series.LineSeries lineSeries2)
            lineSeries2.Color = ColorUtility.HexToOxyColor(series.Colour);
        else if (oxySeries is ScatterSeries scatterSeries2)
            scatterSeries2.MarkerFill = ColorUtility.HexToOxyColor(series.Colour);
        
        return oxySeries;
    }
    
    /// <summary>
    /// Add data points to a line series from a data table.
    /// </summary>
    /// <param name="lineSeries">The line series to add points to.</param>
    /// <param name="modelSeries">The model series containing column information.</param>
    /// <param name="data">The data table containing the data.</param>
    private static void AddDataPoints(OxyPlot.Series.LineSeries lineSeries, ModelOutputSeries modelSeries, DataTable data)
    {
        // Skip if no data or column names are specified
        if (string.IsNullOrEmpty(modelSeries.XAxisColumn) || 
            string.IsNullOrEmpty(modelSeries.YAxisColumn) ||
            !data.Columns.Contains(modelSeries.XAxisColumn) || 
            !data.Columns.Contains(modelSeries.YAxisColumn))
        {
            return;
        }
        
        var points = new List<DataPoint>();
        foreach (DataRow row in data.Rows)
        {
            if (row[modelSeries.XAxisColumn] != DBNull.Value && 
                row[modelSeries.YAxisColumn] != DBNull.Value)
            {
                double x = Convert.ToDouble(row[modelSeries.XAxisColumn]);
                double y = Convert.ToDouble(row[modelSeries.YAxisColumn]);
                points.Add(new DataPoint(x, y));
            }
        }
        
        lineSeries.ItemsSource = points;
    }
    
    /// <summary>
    /// Add scatter points to a scatter series from a data table.
    /// </summary>
    /// <param name="scatterSeries">The scatter series to add points to.</param>
    /// <param name="modelSeries">The model series containing column information.</param>
    /// <param name="data">The data table containing the data.</param>
    private static void AddScatterPoints(ScatterSeries scatterSeries, ModelOutputSeries modelSeries, DataTable data)
    {
        // Skip if no data or column names are specified
        if (string.IsNullOrEmpty(modelSeries.XAxisColumn) || 
            string.IsNullOrEmpty(modelSeries.YAxisColumn) ||
            !data.Columns.Contains(modelSeries.XAxisColumn) || 
            !data.Columns.Contains(modelSeries.YAxisColumn))
        {
            return;
        }
        
        var points = new List<ScatterPoint>();
        foreach (DataRow row in data.Rows)
        {
            if (row[modelSeries.XAxisColumn] != DBNull.Value && 
                row[modelSeries.YAxisColumn] != DBNull.Value)
            {
                double x = Convert.ToDouble(row[modelSeries.XAxisColumn]);
                double y = Convert.ToDouble(row[modelSeries.YAxisColumn]);
                points.Add(new ScatterPoint(x, y));
            }
        }
        
        scatterSeries.ItemsSource = points;
    }
}
