using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A style provider which returns a style based on the series data.
/// </summary>
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
    /// Create a new <see cref="DynamicStyleProvider{T}"/> instance.
    /// </summary>
    /// <param name="identifier">The identifier strategy.</param>
    /// <param name="valueStrategy">The value strategy.</param>
    public DynamicStyleProvider(ISeriesIdentifier identifier, IStyleStrategy<T> valueStrategy)
    {
        Identifier = identifier;
        ValueStrategy = valueStrategy;
    }

    /// <inheritdoc />
    public T GetStyle(ISeriesData data)
    {
        SeriesIdentifierBase identity = Identifier.GetIdentifier(data);
        uint index = GetIndex(identity);
        return ValueStrategy.GetValue(index);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => Identifier.GetStrategy();

    /// <summary>
    /// Get the index for the given identifier. This index will be as unique as
    /// possible, but does not need to be globally unique.
    /// </summary>
    /// <param name="identity">The identifier.</param>
    /// <returns>The index.</returns>
    private uint GetIndex(SeriesIdentifierBase identity)
    {
        int n = ValueStrategy.Count;

        // Primary hash.
        int hash = identity.GetHashCode();

        // If we have a small number of values, use a more sophisticated
        // distribution, to try and avoid collisions.
        if (n < 20)
        {
            string hashString = hash.ToString();
            int hash1 = Math.Abs(hash) % n;
            int hash2 = Math.Abs(hashString.GetHashCode());

            return (uint)((hash1 + hash2 % (n - 1) + 1) % n);
        }

        return (uint)(Math.Abs(hash) % n);
    }
}
