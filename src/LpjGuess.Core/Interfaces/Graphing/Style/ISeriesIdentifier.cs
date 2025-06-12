using System.Text.Json.Serialization;
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
    /// <param name="series">The series.</param>
    /// <returns>An object which uniquely identifies the series according to
    /// this strategy.</returns>
    SeriesIdentityBase Identify(ISeriesData series);

    /// <summary>
    /// Get the strategy that this identifier uses to identify a series.
    /// </summary>
    /// <returns>The strategy used to identify a series.</returns>
    StyleVariationStrategy GetStrategy();
}
