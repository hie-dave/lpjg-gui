using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identifiers;

namespace LpjGuess.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="StyleVariationStrategy"/>.
/// </summary>
public static class StyleVariationStrategyExtensions
{
    /// <summary>
    /// Create a series identifier corresponding to the specified style
    /// variation strategy.
    /// </summary>
    /// <param name="variation">The style variation strategy.</param>
    /// <returns>The series identifier.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static ISeriesIdentifier CreateIdentifier(this StyleVariationStrategy variation)
    {
        return variation switch
        {
            StyleVariationStrategy.ByExperiment => new ExperimentIdentifier(),
            StyleVariationStrategy.ByGridcell => new GridcellIdentifier(),
            StyleVariationStrategy.BySimulation => new SimulationIdentifier(),
            StyleVariationStrategy.ByStand => new StandIdentifier(),
            StyleVariationStrategy.ByPatch => new PatchIdentifier(),
            StyleVariationStrategy.ByIndividual => new IndividualIdentifier(),
            StyleVariationStrategy.ByPft => new PftIdentifier(),
            StyleVariationStrategy.BySeries => new SeriesIdentifier(),
            StyleVariationStrategy.ByLayer => new LayerIdentifier(),
            _ => throw new ArgumentException($"Invalid style variation strategy: {variation}")
        };
    }
}
