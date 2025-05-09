using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which renders a <see cref="DataTable"/>. using a GtkColumnView.
/// </summary>
public class DataTableView : StringColumnView<DataRow>
{
    private ulong nrender = 0;

    /// <summary>
    /// Populate the view.
    /// </summary>
    /// <param name="data"></param>
    public void Populate(DataTable data)
    {
        Clear();
        RemoveColumns();

		foreach (DataColumn column in data.Columns)
			AddColumn(column.ColumnName, row => RenderRow(row, column));
        Populate(data.AsEnumerable());
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
            Console.WriteLine($"Render {nrender++}");
            object value = row[column];
            if (value == null)
                return string.Empty;

            // Round floats to 2 decimal places.
            if (column.DataType == typeof(double) && value is double dbl)
                return dbl.ToString("F2");
            return value.ToString() ?? string.Empty;
        }
        catch (Exception error)
        {
            // If this starts throwing for some reason, we don't want thousands
            // of popup windows to appear. Therefore we just log to stdout for
            // now. Need to revisit to this.
            Console.WriteLine(error);
            return string.Empty;
        }
    }
}
