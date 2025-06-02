using LpjGuess.Core.Models.Graphing.Style;

namespace LpjGuess.Core.Interfaces.Graphing.Style;

/// <summary>
/// An interface to a class which provides a specific kind of style for a
/// series on a graph.
/// </summary>
public interface IStyleProvider
{
    /// <summary>
    /// Initialize the style provider.
    /// </summary>
    /// <param name="totalSeriesCount">The total number of series.</param>
    void Initialize(int totalSeriesCount);

    /// <summary>
    /// Reset the style provider.
    /// </summary>
    void Reset();

    /// <summary>
    /// Get the style variation strategy used by this style provider.
    /// </summary>
    /// <returns>The style variation strategy.</returns>
    StyleVariationStrategy GetStrategy();
}

/// <summary>
/// An interface to a class which provides a specific kind of style for a
/// series on a graph.
/// </summary>
/// <typeparam name="T">The type of the style - e.g. colour, line type,
/// etc.</typeparam>
public interface IStyleProvider<T> : IStyleProvider
{
    /// <summary>
    /// Get a style for the given series data.
    /// </summary>
    /// <param name="data">The series data.</param>
    /// <returns>The style.</returns>
    T GetStyle(ISeriesData data);
}
