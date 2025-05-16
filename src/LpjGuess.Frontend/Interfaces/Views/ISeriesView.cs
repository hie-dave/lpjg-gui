using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view which allows the user to edit a single series.
/// </summary>
public interface ISeriesView<T> : IView where T : ISeries
{
    /// <summary>
    /// Called when the user wants to edit the series.
    /// The event parameter is the action to perform on the series.
    /// </summary>
    Event<IModelChange<T>> OnEditSeries { get; }

    /// <summary>
    /// Populate the view.
    /// </summary>
    /// <param name="series">The series to populate the view with.</param>
    void Populate(T series);
}
