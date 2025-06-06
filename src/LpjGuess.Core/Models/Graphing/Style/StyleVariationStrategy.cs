namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// The ways in which a visual property may vary.
/// </summary>
public enum StyleVariationStrategy
{
    /// <summary>
    /// The visual property does not vary.
    /// </summary>
    Fixed,

    /// <summary>
    /// The visual property varies by simulation.
    /// </summary>
    BySimulation,

    /// <summary>
    /// The visual property varies by gridcell.
    /// </summary>
    ByGridcell,

    /// <summary>
    /// The visual property varies by stand.
    /// </summary>
    ByStand,

    /// <summary>
    /// The visual property varies by patch.
    /// </summary>
    ByPatch,

    /// <summary>
    /// The visual property varies by individual.
    /// </summary>
    ByIndividual,

    /// <summary>
    /// The visual property varies by PFT.
    /// </summary>
    ByPft
}
