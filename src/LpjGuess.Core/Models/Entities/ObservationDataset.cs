namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a dataset containing observations.
/// </summary>
public class ObservationDataset : Dataset
{
    // For reproducibility

    /// <summary>
    /// Source of the observation data.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Version of the dataset.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}
