namespace LpjGuess.Core.Interfaces.Graphing;

/// <summary>
/// An interface to a graph series.
/// </summary>
public interface ISeries
{
    /// <summary>
    /// Series title.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Series colour.
    /// </summary>
    string Colour { get; set; }

    /// <summary>
    /// The source for data displayed in the series.
    /// </summary>
    IDataSource DataSource { get; set; }
}
