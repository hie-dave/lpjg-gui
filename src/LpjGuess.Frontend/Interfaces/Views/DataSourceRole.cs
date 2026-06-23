namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Describes how a data source is used by a graph series.
/// </summary>
public enum DataSourceRole
{
    /// <summary>
    /// The source supplies both intrinsic x coordinates and y values.
    /// </summary>
    Combined,

    /// <summary>
    /// The source supplies plotted x values.
    /// </summary>
    XAxis,

    /// <summary>
    /// The source supplies plotted y values.
    /// </summary>
    YAxis
}
