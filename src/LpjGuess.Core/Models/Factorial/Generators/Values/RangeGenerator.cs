using System.Numerics;
using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Core.Models.Factorial.Generators.Values;

/// <summary>
/// A value generator which generates a range of values.
/// </summary>
public class RangeGenerator<T> : GenericValueGenerator<T>, IRangeGenerator where T : INumber<T>
{
    /// <summary>
    /// The start of the range.
    /// </summary>
    public T Start { get; set; }

    /// <summary>
    /// The number of values to generate.
    /// </summary>
    public int N { get; set; }

    /// <summary>
    /// The step size of the range.
    /// </summary>
    public T Step { get; set; }

    /// <summary>
    /// Create a new <see cref="RangeGenerator{T}"/> instance.
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="n">The number of values to generate.</param>
    /// <param name="step">The step size of the range.</param>
    public RangeGenerator(T start, int n, T step)
    {
        Start = start;
        N = n;
        Step = step;
    }

    /// <inheritdoc />
    public override IEnumerable<T> Generate()
    {
        T value = Start;
        for (int i = 0; i < N; i++)
        {
            yield return value;
            value += Step;
        }
    }

    /// <inheritdoc />
    public override int NumValues() => N;
}
