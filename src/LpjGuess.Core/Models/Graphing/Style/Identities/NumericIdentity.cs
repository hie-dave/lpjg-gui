using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style.Identities;

/// <summary>
/// A class which uniquely identifies a numeric value.
/// </summary>
public class NumericIdentity : SeriesIdentityBase
{
    /// <summary>
    /// The numeric value.
    /// </summary>
    public int Value { get; private init; }

    /// <summary>
    /// Create a new <see cref="NumericIdentity"/> instance.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    public NumericIdentity(int value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public override bool Equals(SeriesIdentityBase? other)
    {
        if (other is not NumericIdentity otherIdentifier)
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
