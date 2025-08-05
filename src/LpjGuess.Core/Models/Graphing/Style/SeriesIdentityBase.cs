namespace LpjGuess.Core.Interfaces.Graphing.Style;

/// <summary>
/// An interface to a class which uniquely identifies a series according to a
/// particular identification strategy.
/// </summary>
/// <remarks>
/// All implementations must be serializable.
/// </remarks>
public abstract class SeriesIdentityBase : IEquatable<SeriesIdentityBase>
{
    /// <inheritdoc />
    public abstract bool Equals(SeriesIdentityBase? other);

    /// <inheritdoc />
    public abstract override int GetHashCode();

    /// <inheritdoc />
    public abstract override string ToString();
}
