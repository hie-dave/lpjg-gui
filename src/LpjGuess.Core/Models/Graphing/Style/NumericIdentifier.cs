using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A class which uniquely identifies a numeric value.
/// </summary>
public class NumericIdentifier : SeriesIdentifierBase
{
    /// <summary>
    /// The numeric value.
    /// </summary>
    public int Value { get; private init; }

    /// <summary>
    /// Create a new <see cref="NumericIdentifier"/> instance.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    public NumericIdentifier(int value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public override bool Equals(SeriesIdentifierBase? other)
    {
        if (other is not NumericIdentifier otherIdentifier)
            return false;

        return Value == otherIdentifier.Value;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
