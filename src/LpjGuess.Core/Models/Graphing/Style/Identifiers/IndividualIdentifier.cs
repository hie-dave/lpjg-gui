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
    public SeriesIdentityBase Identify(SeriesContext context)
    {
        if (context.Individual is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by individual is only valid for individual-level model outputs");

        // TODO: should this take the gridcell/stand/patch into account?
        return Identify(context.Individual.Value);
    }

    /// <summary>
    /// Identify the individual with the given ID.
    /// </summary>
    /// <param name="individual">The ID of the individual.</param>
    /// <returns>The series identity.</returns>
    public SeriesIdentityBase Identify(int individual)
    {
        return new NumericIdentity(individual);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByIndividual;
}
