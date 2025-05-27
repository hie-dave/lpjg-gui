using System.Data;
using System.Runtime.Intrinsics.Arm;
using Dave.Benchmarks.Core.Models.Entities;
using Dave.Benchmarks.Core.Models.Importer;

namespace LpjGuess.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="Quantity"/> objects.
/// </summary>
public static class QuantityExtensions
{
    /// <summary>
    /// Name of the date column in generated data tables.
    /// </summary>
    public const string DateColumn = "Date";

    /// <summary>
    /// Name of the latitude column in generated data tables.
    /// </summary>
    private const string latColumn = "Latitude";

    /// <summary>
    /// Name of the longitude column in generated data tables.
    /// </summary>
    private const string lonColumn = "Longitude";

    /// <summary>
    /// Name of the stand column in generated data tables.
    /// </summary>
    private const string standColumn = "Stand";

    /// <summary>
    /// Name of the patch column in generated data tables.
    /// </summary>
    private const string patchColumn = "Patch";

    /// <summary>
    /// Name of the individual column in generated data tables.
    /// </summary>
    private const string individualColumn = "Individual";

    /// <summary>
    /// Name of the PFT column in generated data tables.
    /// </summary>
    private const string pftColumn = "PFT";

    /// <summary>
    /// Get a data table containing the data in this quantity.
    /// </summary>
    /// <param name="quantity">A model output.</param>
    /// <returns>A <see cref="DataTable"/> representation of the quantity.</returns>
    public static DataTable ToDataTable(this Quantity quantity)
    {
        DataTable table = new DataTable(quantity.Name);

        // Add common columns based on the aggregation level.
        table.Columns.Add(lonColumn, typeof(double));
        table.Columns.Add(latColumn, typeof(double));

        // Note: monthly outputs don't need a date column because they have one
        // column per month, so timestamp is defined per-column rather than per-
        // row.
        if (quantity.Resolution != TemporalResolution.Monthly)
            table.Columns.Add(DateColumn, typeof(DateTime));

        // Add columns based on aggregation level.
        switch (quantity.Level)
        {
            case AggregationLevel.Stand:
                table.Columns.Add(standColumn, typeof(int));
                break;
            case AggregationLevel.Patch:
                table.Columns.Add(standColumn, typeof(int));
                table.Columns.Add(patchColumn, typeof(int));
                break;
            case AggregationLevel.Individual:
                table.Columns.Add(standColumn, typeof(int));
                table.Columns.Add(patchColumn, typeof(int));
                table.Columns.Add(individualColumn, typeof(int));

                // Add PFT column if IndividualPfts is available
                if (quantity.IndividualPfts != null)
                    table.Columns.Add(pftColumn, typeof(string));
                break;
        }

        // Add a column for each layer.
        foreach (Layer layer in quantity.Layers)
        {
            table.Columns.Add(layer.Name, typeof(double));
        }

        // If there are no layers, return the empty table.
        if (!quantity.Layers.Any())
        {
            return table;
        }

        // Get all unique timestamps, longitudes, latitudes, stands, patches,
        // and individuals.
        IEnumerable<DataPoint> dataPoints = quantity.Layers.First().Data;

        Func<DataPoint, DataPoint, bool> timestampMatches;
        if (quantity.Resolution == TemporalResolution.Monthly)
            timestampMatches = (dp, dataPoint) => dp.Timestamp.Year == dataPoint.Timestamp.Year;
        else
            timestampMatches = (p0, p1) => p0.Timestamp == p1.Timestamp;

        // Create rows for each data point.
        // FIXME: this is impractically inefficient with spinup outputs.
        foreach (DataPoint dataPoint in dataPoints)
        {
            DataRow row = table.NewRow();

            // Set common values.
            row[lonColumn] = dataPoint.Longitude;
            row[latColumn] = dataPoint.Latitude;

            if (quantity.Resolution != TemporalResolution.Monthly)
                row[DateColumn] = dataPoint.Timestamp;

            // Set values based on aggregation level.
            switch (quantity.Level)
            {
                case AggregationLevel.Stand:
                    row[standColumn] = dataPoint.Stand ?? 0;
                    break;
                case AggregationLevel.Patch:
                    row[standColumn] = dataPoint.Stand ?? 0;
                    row[patchColumn] = dataPoint.Patch ?? 0;
                    break;
                case AggregationLevel.Individual:
                    row[standColumn] = dataPoint.Stand ?? 0;
                    row[patchColumn] = dataPoint.Patch ?? 0;
                    row[individualColumn] = dataPoint.Individual ?? 0;

                    // Set PFT if available.
                    if (quantity.IndividualPfts != null && dataPoint.Individual.HasValue &&
                        quantity.IndividualPfts.TryGetValue(dataPoint.Individual.Value, out string? pft))
                        row[pftColumn] = pft;
                    break;
            }

            // Set layer values.
            foreach (Layer layer in quantity.Layers)
            {
                // Find the corresponding data point in this layer.
                DataPoint? layerDataPoint = layer.Data.FirstOrDefault(dp =>
                    timestampMatches(dp, dataPoint) &&
                    dp.Longitude == dataPoint.Longitude &&
                    dp.Latitude == dataPoint.Latitude &&
                    dp.Stand == dataPoint.Stand &&
                    dp.Patch == dataPoint.Patch &&
                    dp.Individual == dataPoint.Individual);

                if (layerDataPoint != null)
                    row[layer.Name] = layerDataPoint.Value;
            }

            table.Rows.Add(row);
        }

        return table;
    }
}
