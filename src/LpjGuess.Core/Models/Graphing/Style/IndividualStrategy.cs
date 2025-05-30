using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A strategy for identifying individuals in a deterministic way.
/// </summary>
public class IndividualStrategy : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentifierBase GetIdentifier(ISeriesData series)
    {
        if (series.Context.Individual is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by individual is only valid for individual-level model outputs");

        // TODO: should this take the gridcell/stand/patch into account?
        return new NumericIdentifier(series.Context.Individual.Value);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByIndividual;
}
