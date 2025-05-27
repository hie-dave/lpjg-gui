using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A base class for style strategies.
/// </summary>
public abstract class StyleStrategyBase<T> : IStyleStrategy<T>
{
    /// <summary>
    /// The values to use.
    /// </summary>
    /// <remarks>
    /// TODO: rethink serialization.
    /// </remarks>
    [NonSerialized]
    private readonly IReadOnlyList<T> values;

    /// <summary>
    /// Create a new <see cref="StyleStrategyBase{T}"/> instance.
    /// </summary>
    /// <param name="values">The values to use.</param>
    protected StyleStrategyBase(IEnumerable<T> values)
    {
        this.values = values.ToList();
    }

    /// <inheritdoc />
    public T GetValue(uint index)
    {
        return values[(int)(index % values.Count)];
    }

    /// <inheritdoc />
    public int Count => values.Count;
}

