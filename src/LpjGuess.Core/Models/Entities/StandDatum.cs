using System.ComponentModel.DataAnnotations.Schema;

namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a data point aggregated over all patches in a stand.
/// </summary>
[Table("StandData")]
public class StandDatum : GridcellDatum
{
    /// <summary>
    /// The ID of the stand this data point belongs to.
    /// </summary>
    public int StandId { get; set; }
}
