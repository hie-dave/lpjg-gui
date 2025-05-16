using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// Base interface for presenters that manage series editing.
/// </summary>
public interface ISeriesPresenter : IDisposable
{
    /// <summary>
    /// The series being edited.
    /// </summary>
    ISeries Series { get; }

    /// <summary>
    /// Called when the series has been changed.
    /// </summary>
    Event<ICommand> OnSeriesChanged { get; }

    /// <summary>
    /// Get the view being managed by this presenter.
    /// </summary>
    IView GetView();
}

/// <summary>
/// Generic interface for presenters that manage series editing.
/// </summary>
public interface ISeriesPresenter<T> : ISeriesPresenter, IPresenter<ISeriesView<T>> where T : ISeries
{
    // TODO: do we need a generic series property here? Maybe not, in which case
    // we don't even need this interface.
}
