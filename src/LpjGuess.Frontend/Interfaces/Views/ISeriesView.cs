using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view which allows the user to edit a single series.
/// </summary>
public interface ISeriesView : IView
{
    /// <summary>
    /// Called when the user wants to edit the series.
    /// </summary>
    Event<ICommand> OnEditSeries { get; }
}
