using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// Presenter for managing series editing.
/// </summary>
public class SeriesPresenter<T> : PresenterBase<ISeriesView<T>>, ISeriesPresenter<T> where T : ISeries
{
    /// <summary>
    /// The series being edited.
    /// </summary>
    public T Series { get; private init; }

    /// <summary>
    /// Creates a new instance of SeriesPresenter.
    /// </summary>
    /// <param name="view">The view to present to.</param>
    /// <param name="series">The series being edited.</param>
    public SeriesPresenter(ISeriesView<T> view, T series) : base(view)
    {
        Series = series;
        OnSeriesChanged = new Event<ICommand>();
        view.Populate(series);
        view.OnEditSeries.ConnectTo(OnEditSeries);
    }

    /// <inheritdoc />
    public Event<ICommand> OnSeriesChanged { get; private init; }

    /// <summary>
    /// Implementation of non-generic interface.
    /// </summary>
    ISeries ISeriesPresenter.Series => Series;

    /// <summary>
    /// Implementation of non-generic interface.
    /// </summary>
    IView ISeriesPresenter.GetView() => GetView();

    /// <summary>
    /// Override the default dispose method to additionally disconnect all
    /// event handlers from the event source.
    /// </summary>
    public override void Dispose()
    {
        OnSeriesChanged.DisconnectAll();
        base.Dispose();
    }

    /// <summary>
    /// Called when the series has been changed by the user. Generates a command
    /// object which encapsulates the change, and propagates it up to the owner.
    /// </summary>
    /// <param name="change">The action to perform on the series.</param>
    private void OnEditSeries(IModelChange<T> change)
    {
        ICommand command = change.ToCommand(Series);
        OnSeriesChanged.Invoke(command);
    }
}
