using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Factories;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Factories;
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
    /// The data source presenter factory.
    /// </summary>
    private readonly IDataSourcePresenterFactory factory;

    /// <summary>
    /// Creates a new instance of SeriesPresenter.
    /// </summary>
    /// <param name="view">The view to present to.</param>
    /// <param name="series">The series being edited.</param>
    /// <param name="factory">The data source presenter factory.</param>
    public SeriesPresenter(ISeriesView<T> view, T series, IDataSourcePresenterFactory factory) : base(view)
    {
        Series = series;
        OnSeriesChanged = new Event<ICommand>();
        view.SetAllowedStyleVariationStrategies(GetAllowedStyleVariationStrategies());
        view.Populate(series);
        view.OnEditSeries.ConnectTo(OnEditSeries);
        this.factory = factory;

        IDataSourcePresenter dataSourcePresenter = this.factory.CreatePresenter(series.DataSource);
        dataSourcePresenter.OnDataSourceChanged.ConnectTo(OnDataSourceChanged);
        view.ShowDataSourceView(dataSourcePresenter.GetView());
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
    /// Gets the allowed style variation strategies for the series.
    /// </summary>
    /// <returns>The allowed style variation strategies.</returns>
    private IEnumerable<StyleVariationStrategy> GetAllowedStyleVariationStrategies()
    {
        var strategies = Series.DataSource.GetAllowedStyleVariationStrategies();
        if (!strategies.Contains(StyleVariationStrategy.Fixed))
            strategies = strategies.Append(StyleVariationStrategy.Fixed);
        return strategies;
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

    /// <summary>
    /// Called when the data source has been changed by the user.
    /// </summary>
    /// <param name="command">The action to perform on the data source.</param>
    private void OnDataSourceChanged(ICommand command)
    {
        OnSeriesChanged.Invoke(command);
    }
}
