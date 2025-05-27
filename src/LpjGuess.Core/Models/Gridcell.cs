using System.Diagnostics.CodeAnalysis;

namespace LpjGuess.Core.Models;

/// <summary>
/// A gridcell.
/// </summary>
public class Gridcell
{
    /// <summary>
    /// Optional name of the gridcell.
    /// </summary>
    private readonly string? name;

    /// <summary>
    /// The latitude of the gridcell.
    /// </summary>
    public double Latitude { get; private init; }

    /// <summary>
    /// The longitude of the gridcell.
    /// </summary>
    public double Longitude { get; private init; }

    /// <summary>
    /// Create a new <see cref="Gridcell"/> instance.
    /// </summary>
    public Gridcell(double latitude, double longitude, string? name = null)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
        this.name = name;
    }

    /// <summary>
    /// Get the name of the gridcell.
    /// </summary>
    public string Name => name ?? $"({Latitude}, {Longitude})";

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Gridcell other)
            return false;

        return Latitude == other.Latitude &&
               Longitude == other.Longitude;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Latitude, Longitude, name);
    }
}
