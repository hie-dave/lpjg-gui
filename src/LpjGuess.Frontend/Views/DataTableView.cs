using System.Data;
using LpjGuess.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which renders a <see cref="DataTable"/>. using a GtkColumnView.
/// </summary>
public class DataTableView : StringColumnView<DataRow>
{
    /// <summary>
    /// Date format used for yearly data.
    /// </summary>
    private const string yearlyFormat = "yyyy";

    /// <summary>
    /// Date format used for daily data.
    /// </summary>
    private const string dailyFormat = "yyyy-MM-dd";

    /// <summary>
    /// Date format used for subdaily data.
    /// </summary>
    private const string hourlyFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<DataTableView> logger;

    /// <summary>
    /// Number of decimal digits to display in the table.
    /// </summary>
    public int Precision { get; set; } = 2;

    /// <summary>
    /// The date format to be used.
    /// </summary>
    private string? dateFormat = null;

    /// <summary>
    /// Create a new <see cref="DataTableView"/> instance.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public DataTableView(ILogger<DataTableView> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Populate the view.
    /// </summary>
    /// <param name="data">A data table.</param>
    public void Populate(DataTable data)
    {
        Clear();

        // TODO: this logic doesn't belong in a view. Need to move it.
        if (data.Columns.Contains(QuantityExtensions.DateColumn))
        {
            // Try to choose a good date format based on the data.
            dateFormat = GetDateFormat(data);
        }

        foreach (DataColumn column in data.Columns)
            AddColumn(column.ColumnName, row => RenderRow(row, column));
        Populate(data.AsEnumerable());
    }

    /// <summary>
    /// Get a suitable date format for the given data table.
    /// </summary>
    /// <param name="data">The data table.</param>
    /// <returns>A date format.</returns>
    private string GetDateFormat(DataTable data)
    {
        // If there is exactly one value per year, display only year.
        if (data.AsEnumerable()
                .Select(row => (DateTime)row[QuantityExtensions.DateColumn])
                .GroupBy(date => date.Year)
                .All(group => group.Count() == 1))
            return yearlyFormat;

        // If all dates have the same timestamp, only display the
        // date (ie don't display the time).
        if (data.AsEnumerable().All(row => row[QuantityExtensions.DateColumn] is DateTime date && date.Date == date))
            return dailyFormat;

        return hourlyFormat;
    }

    /// <summary>
    /// Render a particular element of a row to a string value.
    /// </summary>
    /// <param name="row">The row to render.</param>
    /// <param name="column">The column to render.</param>
    /// <returns>A string representation of the specified column in this row.</returns>
    private string RenderRow(DataRow row, DataColumn column)
    {
        try
        {
            object value = row[column];
            if (value == null)
                return string.Empty;

            // Round floats to the specified number of decimal places.
            if (value is double dbl)
                return dbl.ToString($"F{Precision}");

            if (dateFormat != null && value is DateTime date)
                return date.ToString(dateFormat);

            return value.ToString() ?? string.Empty;
        }
        catch (Exception error)
        {
            // If this starts throwing for some reason, we don't want thousands
            // of popup windows to appear. Therefore we just log to stdout for
            // now. Need to revisit to this.
            logger.LogError(error, "Failed to render row");
            return string.Empty;
        }
    }
}
