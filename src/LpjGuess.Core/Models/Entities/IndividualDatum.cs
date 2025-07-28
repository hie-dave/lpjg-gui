using System.ComponentModel.DataAnnotations.Schema;

namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a data point for an individual plant.
/// </summary>
[Table("IndividualData")]
public class IndividualDatum : PatchDatum
{
    /// <summary>
    /// The ID of the individual this data point belongs to.
    /// </summary>
    public int IndividualId { get; set; }

    /// <summary>
    /// Navigation property for the individual this data point belongs to.
    /// </summary>
    public Individual Individual { get; set; } = null!;
}
