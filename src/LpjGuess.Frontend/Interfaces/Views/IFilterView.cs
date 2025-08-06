using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Interface to a view for editing a single graph filter.
/// </summary>
public interface IFilterView : IView
{
    /// <summary>
    /// The strategy used to filter data.
    /// </summary>
    StyleVariationStrategy Strategy { get; }

    /// <summary>
    /// Populate the view with the given series identities.
    /// </summary>
    /// <param name="strategy">The strategy used to filter data.</param>
    /// <param name="identities">A collection of tuples of series identity and whether that series is enabled.</param>
    void Populate(StyleVariationStrategy strategy, IEnumerable<(string identity, bool enabled)> identities);

    /// <summary>
    /// The event that is raised when the filter is changed by the user. The
    /// event parameter is the new set of names of enabled series identities.
    /// </summary>
    Event<IEnumerable<string>> OnFilterChanged { get; }
}
