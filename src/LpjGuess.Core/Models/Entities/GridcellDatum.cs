using System.ComponentModel.DataAnnotations.Schema;

namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a data point aggregated over all stands and patches in a gridcell.
/// </summary>
[Table("GridcellData")]
public class GridcellDatum : Datum
{
    // No additional fields needed for gridcell level
}
