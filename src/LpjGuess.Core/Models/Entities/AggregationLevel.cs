namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Defines the level at which data is aggregated.
/// </summary>
public enum AggregationLevel
{
    /// <summary>
    /// Gridcell-level data, with one row per gridcell.
    /// </summary>
    Gridcell,

    /// <summary>
    /// Stand-level data, with one row per stand.
    /// </summary>
    Stand,

    /// <summary>
    /// Patch-level data, with one row per patch.
    /// </summary>
    Patch,

    /// <summary>
    /// Individual-level data, with one row per individual.
    /// </summary>
    Individual
}
