using System.Diagnostics.CodeAnalysis;
using LpjGuess.Core.Utility;

namespace LpjGuess.Core.Models;

/// <summary>
/// A gridcell.
/// </summary>
public class Gridcell
{
    /// <summary>
    /// Tolerance for floating point comparison.
    /// </summary>
    private readonly double eps;

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
    /// <param name="latitude">The latitude of the gridcell.</param>
    /// <param name="longitude">The longitude of the gridcell.</param>
    /// <param name="name">Optional name of the gridcell.</param>
    /// <param name="eps">Tolerance for floating point comparison.</param>
    /// <remarks>
    /// Default tolerance of 1e-2, because coordinates in output files are
    /// usually rounded to 2 decimal places.
    /// </remarks>
    public Gridcell(
        double latitude,
        double longitude,
        string? name = null,
        double eps = 1e-2)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
        this.name = name;
        this.eps = eps;
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

        return MathUtility.AreEqual(Latitude, other.Latitude, eps) &&
               MathUtility.AreEqual(Longitude, other.Longitude, eps);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Latitude, Longitude, name);
    }
}
