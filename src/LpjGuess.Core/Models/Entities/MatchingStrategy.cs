namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a strategy for matching datapoints between datasets.
/// </summary>
public enum MatchingStrategy
{
    /// <summary>
    /// Match all datapoints in the two datasets irrespective of their
    /// coordinates, provided both datasets have the same simulation ID.
    /// </summary>
    ByName,

    /// <summary>
    /// Match datapoints in the two datasets based on their coordinates. For
    /// each datapoint in the first dataset, find the nearest datapoint in the
    /// second dataset within a certain distance threshold.
    /// </summary>
    Nearest,

    /// <summary>
    /// Match datapoints in the two datasets based on their coordinates. Only
    /// match datapoints that have exactly the same coordinates.
    /// </summary>
    ExactMatch
}
