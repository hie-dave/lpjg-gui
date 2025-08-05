using LpjGuess.Core.Models;

namespace LpjGuess.Core.Interfaces.Graphing;

/// <summary>
/// A data filter.
/// </summary>
public interface IDataFilter
{
    /// <summary>
    /// Check if the given series is filtered.
    /// </summary>
    /// <param name="context">The context describing the series data.</param>
    /// <returns>True if the series is filtered (ie ignored), false otherwise.</returns>
    bool IsFiltered(SeriesContext context);
}
