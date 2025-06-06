using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using Newtonsoft.Json;

namespace LpjGuess.Core.Models.Graphing.Style.Providers;

/// <summary>
/// A style provider which always returns the same style.
/// </summary>
public class FixedStyleProvider<T> : IStyleProvider<T>
{
    /// <summary>
    /// The style to return.
    /// </summary>
    public T Style { get; private init; }

    /// <summary>
    /// Create a new <see cref="FixedStyleProvider{T}"/> instance.
    /// </summary>
    /// <param name="style">The style to return.</param>
    [JsonConstructor]
    public FixedStyleProvider(T style) => Style = style;

    /// <inheritdoc />
    public T GetStyle(ISeriesData data) => Style;

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.Fixed;

    /// <inheritdoc />
    /// <remarks>
    /// This method does nothing for a fixed style provider.
    /// </remarks>
    public void Initialize(int totalSeriesCount) { }

    /// <inheritdoc />
    public void Reset() { }
}
