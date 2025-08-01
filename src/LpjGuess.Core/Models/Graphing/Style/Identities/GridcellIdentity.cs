using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Utility;

namespace LpjGuess.Core.Models.Graphing.Style.Identities;

/// <summary>
/// A class which uniquely identifies a gridcell based on its latitude and
/// longitude.
/// </summary>
public class GridcellIdentity : SeriesIdentityBase
{
    /// <summary>
    /// The latitude of the gridcell.
    /// </summary>
    public double Latitude { get; private init; }

    /// <summary>
    /// The longitude of the gridcell.
    /// </summary>
    public double Longitude { get; private init; }

    /// <summary>
    /// The name of the gridcell.
    /// </summary>
    public string? Name { get; private init; }

    /// <summary>
    /// Create a new <see cref="GridcellIdentity"/> instance.
    /// </summary>
    /// <param name="latitude">The latitude of the gridcell.</param>
    /// <param name="longitude">The longitude of the gridcell.</param>
    /// <param name="name">The name of the gridcell.</param>
    public GridcellIdentity(double latitude, double longitude, string? name = null)
    {
        Latitude = latitude;
        Longitude = longitude;
        Name = name;
    }

    /// <inheritdoc />
    public override bool Equals(SeriesIdentityBase? other)
    {
        if (other is not GridcellIdentity otherIdentifier)
            return false;

        return MathUtility.AreEqual(Latitude, otherIdentifier.Latitude) &&
               MathUtility.AreEqual(Longitude, otherIdentifier.Longitude);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Name is not included in the hash code, so that two gridcell
        // identifiers with the same latitude and longitude are considered
        // equal.
        return HashCode.Combine(Latitude, Longitude);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name ?? $"{Latitude}, {Longitude}";
    }
}
