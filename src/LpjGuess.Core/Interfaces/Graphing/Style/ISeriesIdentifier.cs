using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing.Style;

namespace LpjGuess.Core.Interfaces.Graphing.Style;

/// <summary>
/// A strategy for identifying series in a deterministic way.
/// </summary>
public interface ISeriesIdentifier
{
    /// <summary>
    /// Identify a series.
    /// </summary>
    /// <param name="context">The context describing the series data.</param>
    /// <returns>An object which uniquely identifies the series according to
    /// this strategy.</returns>
    SeriesIdentityBase Identify(SeriesContext context);

    /// <summary>
    /// Get the strategy that this identifier uses to identify a series.
    /// </summary>
    /// <returns>The strategy used to identify a series.</returns>
    StyleVariationStrategy GetStrategy();
}
