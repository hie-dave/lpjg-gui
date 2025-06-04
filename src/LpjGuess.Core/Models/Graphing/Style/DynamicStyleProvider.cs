using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using Newtonsoft.Json;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A style provider which cycles between a finite number of style values for
/// a particular type of style.
/// </summary>
/// <typeparam name="T">The type of the style.</typeparam>
public class DynamicStyleProvider<T> : IStyleProvider<T>
{
    /// <summary>
    /// The strategy used to identify a series. The series' identity is used by
    /// the value strategy to get a style value.
    /// </summary>
    public ISeriesIdentifier Identifier { get; private init; }

    /// <summary>
    /// The strategy to use to select the value for a given index.
    /// </summary>
    public IStyleStrategy<T> ValueStrategy { get; private init; }

    /// <summary>
    /// The styles for each series.
    /// </summary>
    private readonly Dictionary<SeriesIdentifierBase, T> styles;

    /// <summary>
    /// The current index.
    /// </summary>
    private uint index;

    /// <summary>
    /// Create a new <see cref="DynamicStyleProvider{T}"/> instance.
    /// </summary>
    /// <param name="identifier">The identifier strategy.</param>
    /// <param name="valueStrategy">The value strategy.</param>
    [JsonConstructor]
    public DynamicStyleProvider(ISeriesIdentifier identifier, IStyleStrategy<T> valueStrategy)
    {
        Identifier = identifier;
        ValueStrategy = valueStrategy;
        styles = new Dictionary<SeriesIdentifierBase, T>();
        index = 0;
    }

    /// <inheritdoc />
    public T GetStyle(ISeriesData data)
    {
        lock (styles)
        {
            if (!styles.TryGetValue(Identifier.GetIdentifier(data), out T? style))
            {
                style = ValueStrategy.GetValue(index++);
                styles.Add(Identifier.GetIdentifier(data), style);
            }
            return style;
        }
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => Identifier.GetStrategy();

    /// <inheritdoc />
    public virtual void Initialize(int totalSeriesCount)
    {
        Reset();
    }

    /// <inheritdoc />
    public virtual void Reset()
    {
        styles.Clear();
        index = 0;
    }
}
