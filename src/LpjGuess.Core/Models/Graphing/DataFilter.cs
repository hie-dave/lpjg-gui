using LpjGuess.Core.Extensions;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style;

namespace LpjGuess.Core.Models.Graphing;

/// <summary>
/// A data filter.
/// </summary>
public class DataFilter : IDataFilter
{
    /// <summary>
    /// The strategy to use for filtering.
    /// </summary>
    public StyleVariationStrategy Strategy { get; set; }

    /// <summary>
    /// The values (identified by the given strategy) to be ignored/filtered out.
    /// </summary>
    public List<SeriesIdentityBase> FilteredValues { get; set; }

    /// <summary>
    /// Create a new <see cref="DataFilter"/> instance.
    /// </summary>
    public DataFilter()
    {
        FilteredValues = [];
    }

    /// <inheritdoc />
    public bool IsFiltered(SeriesContext context)
    {
        ISeriesIdentifier identifier = Strategy.CreateIdentifier();
        return FilteredValues.Contains(identifier.Identify(context));
    }
}
