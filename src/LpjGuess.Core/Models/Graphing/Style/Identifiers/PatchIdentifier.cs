using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identities;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies a series by its patch (only
/// applicable for a series containing patch-level data).
/// </summary>
public class PatchIdentifier : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentityBase Identify(SeriesContext context)
    {
        if (context.Patch is null)
            // This should never happen - users should only be able to select
            // this strategy for series on which it is valid.
            throw new InvalidOperationException("Varying by patch is only valid for patch-level model outputs");

        // TODO: should this take the gridcell/stand into account?
        return new NumericIdentity(context.Patch.Value);
    }

    /// <summary>
    /// Identify a series by its patch ID.
    /// </summary>
    /// <param name="patch">The ID of the patch.</param>
    /// <returns>The series identity.</returns>
    public SeriesIdentityBase Identify(int patch)
    {
        return new NumericIdentity(patch);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByPatch;
}
