using LpjGuess.Core.Models.Graphing;

namespace LpjGuess.Core.Interfaces;

/// <summary>
/// An interface to a data source.
/// </summary>
public interface IDataSource
{
    /// <summary>
    /// Get the x-axis type for the data source.
    /// </summary>
    /// <returns>The x-axis type for the data source.</returns>
    AxisType GetXAxisType();

    /// <summary>
    /// Get the y-axis type for the data.
    /// </summary>
    /// <returns>The y-axis type for the data.</returns>
    AxisType GetYAxisType();

    /// <summary>
    /// Get a title for the x-axis of this data.
    /// </summary>
    /// <returns>The x-axis title.</returns>
    string GetXAxisTitle();

    /// <summary>
    /// Get a title for the y-axis of this data.
    /// </summary>
    /// <returns>The y-axis title.</returns>
    string GetYAxisTitle();

    /// <summary>
    /// Get a name which describes the data plotted by this source.
    /// </summary>
    /// <returns>The name of the data source.</returns>
    string GetName();
}
