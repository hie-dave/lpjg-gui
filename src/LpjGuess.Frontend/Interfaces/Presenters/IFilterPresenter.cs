using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// Interface to a presenter for editing a single graph filter.
/// </summary>
public interface IFilterPresenter : IPresenter<IFilterView, DataFilter>
{
    /// <summary>
    /// Event raised when the filter is changed by the user.
    /// </summary>
    Event<ICommand> OnFilterChanged { get; }

    /// <summary>
    /// Populate the view with the given series identities.
    /// </summary>
    /// <param name="identities">The series identities to populate the view with.</param>
    void Populate(IEnumerable<SeriesIdentityBase> identities);
}
