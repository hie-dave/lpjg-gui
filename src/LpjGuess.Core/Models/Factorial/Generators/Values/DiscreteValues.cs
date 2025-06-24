namespace LpjGuess.Core.Models.Factorial.Generators.Values;

/// <summary>
/// A value generator which generates a set of discrete values.
/// </summary>
public class DiscreteValues<T> : GenericValueGenerator<T>
{
    /// <summary>
    /// The values to be generated.
    /// </summary>
    public List<T> Values { get; set; }

    /// <summary>
    /// Create a new <see cref="DiscreteValues{T}"/> instance.
    /// </summary>
    /// <param name="values">The values to be generated.</param>
    public DiscreteValues(IEnumerable<T> values) => Values = values.ToList();

    /// <inheritdoc />
    public override IEnumerable<T> Generate() => Values;

    /// <inheritdoc />
    public override int NumValues() => Values.Count;
}
