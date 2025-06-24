using System.Collections;
using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Core.Models.Factorial.Generators.Values;

/// <summary>
/// Abstract base class for value generators.
/// </summary>
public abstract class GenericValueGenerator<T> : IValueGenerator<T>
{
    /// <inheritdoc />
    public abstract IEnumerable<T> Generate();

    /// <inheritdoc />
    IEnumerable IValueGenerator.Generate() => Generate();

    /// <inheritdoc />
    public virtual IEnumerable<string> GenerateStrings(IFormatProvider? provider)
    {
        foreach (object? value in Generate())
        {
            if (value is IFormattable formattable)
                yield return formattable.ToString(null, provider);
            else
                yield return value?.ToString() ?? string.Empty;
        }
    }

    /// <inheritdoc />
    public abstract int NumValues();
}
