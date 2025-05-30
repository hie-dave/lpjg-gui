using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

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
    public FixedStyleProvider(T style) => Style = style;

    /// <inheritdoc />
    public T GetStyle(ISeriesData data) => Style;

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.Fixed;
}
