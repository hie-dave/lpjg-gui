using System.ComponentModel.DataAnnotations.Schema;

namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a data point for an individual patch.
/// </summary>
[Table("PatchData")]
public class PatchDatum : StandDatum
{
    /// <summary>
    /// The ID of the patch this data point belongs to.
    /// </summary>
    public int PatchId { get; set; }
}
