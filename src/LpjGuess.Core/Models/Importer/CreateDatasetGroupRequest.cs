using System.ComponentModel.DataAnnotations;

namespace LpjGuess.Core.Models.Importer;

/// <summary>
/// Request model for creating a new dataset group.
/// </summary>
public class CreateDatasetGroupRequest
{
    /// <summary>
    /// Name of the group.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the group.
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata about this group.
    /// </summary>
    public string Metadata { get; set; } = "{}";
}
