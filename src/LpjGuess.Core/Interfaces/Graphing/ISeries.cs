using LpjGuess.Core.Models.Graphing;

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

    /// <summary>
    /// The position of the x-axis for the series.
    /// </summary>
    AxisPosition XAxisPosition { get; set; }

    /// <summary>
    /// The position of the y-axis for the series.
    /// </summary>
    AxisPosition YAxisPosition { get; set; }

    /// <summary>
    /// Get the axis requirements for the series.
    /// </summary>
    /// <returns>The axis requirements for the series.</returns>
    IEnumerable<AxisRequirements> GetAxisRequirements();
}
