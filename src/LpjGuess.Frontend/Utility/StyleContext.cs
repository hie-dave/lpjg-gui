using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Frontend.Utility;

/// <summary>
/// A context for style providers.
/// </summary>
public class StyleContext
{
    /// <summary>
    /// The style providers managed by this context.
    /// </summary>
    private readonly List<IStyleProvider> providers;

    /// <summary>
    /// The total number of series managed by this context.
    /// </summary>
    private readonly int totalSeriesCount;

    /// <summary>
    /// Create a new <see cref="StyleContext"/> instance.
    /// </summary>
    /// <param name="totalSeriesCount">The total number of series.</param>
    public StyleContext(int totalSeriesCount)
    {
        providers = new List<IStyleProvider>();
        this.totalSeriesCount = totalSeriesCount;
    }

    /// <summary>
    /// Get a style for the given series data.
    /// </summary>
    /// <typeparam name="T">The type of the style.</typeparam>
    /// <param name="provider">The style provider.</param>
    /// <param name="data">The series data.</param>
    /// <returns>The style.</returns>
    public T GetStyle<T>(IStyleProvider<T> provider, ISeriesData data)
    {
        if (!providers.Contains(provider))
        {
            provider.Initialize(totalSeriesCount);
            providers.Add(provider);
        }
        return provider.GetStyle(data);
    }
}
