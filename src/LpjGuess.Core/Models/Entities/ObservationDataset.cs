namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a dataset containing observations.
/// </summary>
public class ObservationDataset : Dataset
{
    /// <summary>
    /// Additional metadata about this observation dataset stored as a JSON document.
    /// </summary>
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Source of the observation data.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Version of the dataset.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The strategy to use when matching points in this observation dataset to
    /// a prediction dataset.
    /// </summary>
    public MatchingStrategy MatchingStrategy { get; set; }

    /// <summary>
    /// The maximum distance (in km) to use when matching points in this
    /// observation dataset to a prediction dataset, if the MatchingStrategy is
    /// set to Nearest.
    /// </summary>
    public int? MaxDistance { get; set; }

    /// <summary>
    /// Whether this observation dataset is to be used for evaluation of
    /// predictions.
    /// </summary>
    public bool Active { get; set; }
}
