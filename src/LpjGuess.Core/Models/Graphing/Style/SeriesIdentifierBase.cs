namespace LpjGuess.Core.Interfaces.Graphing.Style;

/// <summary>
/// An interface to a class which uniquely identifies a series according to a
/// particular identification strategy.
/// </summary>
public abstract class SeriesIdentifierBase : IEquatable<SeriesIdentifierBase>
{
    /// <inheritdoc />
    public abstract bool Equals(SeriesIdentifierBase? other);

    /// <inheritdoc />
    public abstract override int GetHashCode();

    /// <inheritdoc />
    public abstract override string ToString();
}
