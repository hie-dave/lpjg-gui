using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identities;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies a series by its individual (only
/// applicable for a series containing individual-level data).
/// </summary>
public class IndividualIdentifier : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentityBase Identify(ISeriesData series)
    {
        if (series.Context.Individual is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by individual is only valid for individual-level model outputs");

        // TODO: should this take the gridcell/stand/patch into account?
        return new NumericIdentity(series.Context.Individual.Value);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByIndividual;
}
