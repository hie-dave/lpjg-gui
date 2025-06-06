using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A class which uniquely identifies a coordinate based on its name.
/// </summary>
public class StringIdentity : SeriesIdentityBase
{
    /// <summary>
    /// The name of the coordinate.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// Create a new <see cref="StringIdentity"/> instance.
    /// </summary>
    /// <param name="name">The name of the coordinate.</param>
    public StringIdentity(string name) => Name = name;

    /// <inheritdoc />
    public override bool Equals(SeriesIdentityBase? other)
    {
        if (other is not StringIdentity otherIdentifier)
            return false;

        return Name == otherIdentifier.Name;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }
}
