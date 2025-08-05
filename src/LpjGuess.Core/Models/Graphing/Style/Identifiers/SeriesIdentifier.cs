using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identities;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies every series uniquely.
/// </summary>
public class SeriesIdentifier : ISeriesIdentifier
{
    private int index = 0;

    /// <inheritdoc />
    public SeriesIdentityBase Identify(SeriesContext context)
    {
        // TODO: should this take the gridcell/stand/patch into account?
        lock (this)
            return new NumericIdentity(index++);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.BySeries;
}
