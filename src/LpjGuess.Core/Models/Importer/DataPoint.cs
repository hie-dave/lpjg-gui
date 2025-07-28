namespace LpjGuess.Core.Models.Importer;

/// <summary>
/// Represents a single data point in time
/// </summary>
/// <param name="Timestamp">The timestamp of the data point.</param>
/// <param name="Longitude">The longitude in degrees.</param>
/// <param name="Latitude">The latitude in degrees.</param>
/// <param name="Value">The value at this point.</param>
/// <param name="Stand">Stand ID, if applicable.</param>
/// <param name="Patch">Patch ID, if applicable.</param>
/// <param name="Individual">Individual ID, if applicable.</param>
public record DataPoint(
    DateTime Timestamp,
    double Longitude,
    double Latitude,
    double Value,
    int? Stand = null,
    int? Patch = null,
    int? Individual = null);
