using System.ComponentModel.DataAnnotations;

namespace LpjGuess.Core.Models.Importer;

/// <summary>
/// Request model for creating a new prediction dataset.
/// </summary>
public class CreateDatasetRequest
{
    /// <summary>
    /// Name of the dataset.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the dataset.
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Version of the model used to generate this dataset.
    /// </summary>
    [Required]
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>
    /// Climate dataset used for the model run.
    /// </summary>
    [Required]
    public string ClimateDataset { get; set; } = string.Empty;

    /// <summary>
    /// Temporal resolution of the data (e.g., "daily", "monthly").
    /// </summary>
    [Required]
    public string TemporalResolution { get; set; } = string.Empty;

    /// <summary>
    /// Compressed code patches applied to the model.
    /// </summary>
    [Required]
    public byte[] CompressedCodePatches { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Additional metadata about this dataset.
    /// For site-level datasets, this should include site location and characteristics.
    /// For gridded datasets, this should include spatial extent and resolution.
    /// </summary>
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Optional ID of the group to add this dataset to.
    /// </summary>
    public int? GroupId { get; set; }
}
