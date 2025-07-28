using System.Collections.Generic;

namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents an individual plant in the model.
/// </summary>
public class Individual
{
    /// <summary>
    /// The unique identifier for this individual across all datasets.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The individual's identifier within its simulation (dataset).
    /// This is the ID that appears in the model output files.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// The ID of the dataset this individual belongs to.
    /// </summary>
    public int DatasetId { get; set; }

    /// <summary>
    /// Navigation property for the dataset this individual belongs to.
    /// </summary>
    public Dataset Dataset { get; set; } = null!;

    /// <summary>
    /// The ID of the PFT this individual belongs to.
    /// </summary>
    public int PftId { get; set; }

    /// <summary>
    /// Navigation property for the PFT this individual belongs to.
    /// </summary>
    public Pft Pft { get; set; } = null!;

    /// <summary>
    /// Navigation property for data points associated with this individual.
    /// </summary>
    public ICollection<IndividualDatum> Data { get; set; } = new List<IndividualDatum>();
}
