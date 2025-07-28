using System;

namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Base class for all data points.
/// </summary>
public abstract class Datum
{
    /// <summary>
    /// The unique identifier for this data point.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The value at this data point.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// When this data point was recorded.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The longitude in degrees.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// The latitude in degrees.
    /// </summary>
    public double Latitude { get; set; }

    // Navigation properties
    /// <summary>
    /// The ID of the variable this data point belongs to.
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// Navigation property for the variable this data point belongs to.
    /// </summary>
    public Variable Variable { get; set; } = null!;

    /// <summary>
    /// The ID of the layer this data point belongs to.
    /// </summary>
    public int LayerId { get; set; }

    /// <summary>
    /// Navigation property for the layer this data point belongs to.
    /// </summary>
    public VariableLayer Layer { get; set; } = null!;
}
