using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
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
public class SeriesPresenter<T> : PresenterBase<ISeriesView<T>, T>, ISeriesPresenter<T> where T : ISeries
{
    /// <summary>
    /// The series being edited.
    /// </summary>
    public T Series { get; private init; }

    /// <summary>
    /// The data source presenter factory.
    /// </summary>
    private readonly IDataSourcePresenterFactory factory;
    private readonly SeriesValidationCommandFactory validationCommandFactory;

    /// <summary>
    /// Creates a new instance of SeriesPresenter.
    /// </summary>
    /// <param name="view">The view to present to.</param>
    /// <param name="series">The series being edited.</param>
    /// <param name="factory">The data source presenter factory.</param>
    /// <param name="registry">The command registry.</param>
    public SeriesPresenter(
        ISeriesView<T> view,
        T series,
        IDataSourcePresenterFactory factory,
        ICommandRegistry registry) : base(view, series, registry)
    {
        Series = series;
        OnSeriesChanged = new Event<ICommand>();
        view.SetAllowedStyleVariationStrategies(GetAllowedStyleVariationStrategies());
        view.Populate(series);
        view.OnEditSeries.ConnectTo(OnEditSeries);
        this.factory = factory;
        this.validationCommandFactory = new SeriesValidationCommandFactory();

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
    /// Invoke a composite command containing the specified command and a
    /// validation command for the series.
    /// </summary>
    /// <param name="command">The command to update the series with.</param>
    private void UpdateSeries(ICommand command)
    {
        OnSeriesChanged.Invoke(new CompositeCommand([
            command,
            validationCommandFactory.CreateValidationCommand(Series)
        ]));
    }

    /// <summary>
    /// Called when the series has been changed by the user. Generates a command
    /// object which encapsulates the change, and propagates it up to the owner.
    /// </summary>
    /// <param name="change">The action to perform on the series.</param>
    private void OnEditSeries(IModelChange<T> change)
    {
        UpdateSeries(change.ToCommand(Series));
    }

    /// <summary>
    /// Called when the data source has been changed by the user.
    /// </summary>
    /// <param name="command">The action to perform on the data source.</param>
    private void OnDataSourceChanged(ICommand command)
    {
        UpdateSeries(command);
    }
}
