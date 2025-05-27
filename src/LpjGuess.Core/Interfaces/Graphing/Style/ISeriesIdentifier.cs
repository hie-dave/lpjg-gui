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
    SeriesIdentifierBase GetIdentifier(ISeriesData series);
}
