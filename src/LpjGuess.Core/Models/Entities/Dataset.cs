using System;
using System.Collections.Generic;

namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a collection of data points.
/// </summary>
public abstract class Dataset
{
    /// <summary>
    /// Unique identifier for this dataset.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of this dataset.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of this dataset.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The time this dataset was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The spatial resolution of this dataset.
    /// </summary>
    public string SpatialResolution { get; set; } = string.Empty;

    /// <summary>
    /// The temporal resolution of this dataset.
    /// </summary>
    public string TemporalResolution { get; set; } = string.Empty;

    // Navigation properties

    /// <summary>
    /// The variables in this dataset.
    /// </summary>
    public ICollection<Variable> Variables { get; set; } = new List<Variable>();

    /// <summary>
    /// The group this dataset belongs to, if any.
    /// </summary>
    public DatasetGroup? Group { get; set; }

    /// <summary>
    /// The ID of the group this dataset belongs to, if any.
    /// </summary>
    public int? GroupId { get; set; }

    /// <summary>
    /// The simulation ID associated with this dataset. This is a well-defined
    /// name that is persistent through time. It's not associated with a
    /// specific run of the model, but rather is used to match data from a run
    /// of the model with this simulation ID, to data from an observation
    /// dataset with this simulation ID.
    /// </summary>
    /// <remarks>
    /// E.g. "CumberlandPlain" for the CumberlandPlain site, or
    /// "dave-barra2-1deg" for a (hypothetical) 1-degree continental model run
    /// using the BARRA2 dataset.
    /// </remarks>
    public string SimulationId { get; set; } = string.Empty;
}
