using LpjGuess.Core.Interfaces.Graphing.Style;
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
    IStyleProvider<Colour> ColourProvider { get; set; }

    /// <summary>
    /// An optional independent source for values displayed on the x-axis.
    /// When null, the intrinsic x-values from <see cref="YDataSource"/> are
    /// used.
    /// </summary>
    IDataSource? XDataSource { get; set; }

    /// <summary>
    /// The source for values displayed on the y-axis.
    /// </summary>
    IDataSource YDataSource { get; set; }

    /// <summary>
    /// Backwards-compatible alias for <see cref="YDataSource"/>.
    /// Assigning this property also clears <see cref="XDataSource"/>.
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
