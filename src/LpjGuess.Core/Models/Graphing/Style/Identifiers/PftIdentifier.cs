using LpjGuess.Core.Interfaces.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies a series by its PFT (only
/// applicable for a series containing cohort-level data).
/// </summary>
public class PftIdentifier : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentityBase Identify(SeriesContext context)
    {
        if (context.Pft is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by PFT is only valid for cohort-level model outputs");

        // TODO: should this take the gridcell/stand/patch into account?
        return Identify(context.Pft);
    }

    /// <summary>
    /// Identify a series by its PFT.
    /// </summary>
    /// <param name="pft">The PFT.</param>
    /// <returns>The series identity.</returns>
    public SeriesIdentityBase Identify(string pft)
    {
        return new StringIdentity(pft);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByPft;
}
