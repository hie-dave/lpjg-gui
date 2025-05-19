using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Frontend.Interfaces.Factories;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Factories;

/// <summary>
/// Factory for creating series presenters based on series type.
/// </summary>
public class SeriesPresenterFactory : ISeriesPresenterFactory
{
    /// <summary>
    /// The data source presenter factory.
    /// </summary>
    private readonly IDataSourcePresenterFactory dataSourcePresenterFactory;

    /// <summary>
    /// Creates a new instance of SeriesPresenterFactory.
    /// </summary>
    /// <param name="dataSourcePresenterFactory">The data source presenter factory.</param>
    public SeriesPresenterFactory(IDataSourcePresenterFactory dataSourcePresenterFactory)
    {
        this.dataSourcePresenterFactory = dataSourcePresenterFactory;
    }

    /// <summary>
    /// Creates an appropriate presenter for the given series.
    /// </summary>
    /// <param name="series">The series to create a presenter for.</param>
    /// <returns>A presenter compatible with the series type.</returns>
    public ISeriesPresenter CreatePresenter(ISeries series)
    {
        return series switch
        {
            LineSeries lineSeries => CreateLineSeriesPresenter(lineSeries),
            _ => throw new ArgumentException($"No presenter available for series type {series.GetType().Name}")
        };
    }

    /// <summary>
    /// Create a presenter for a line series view.
    /// </summary>
    /// <param name="series">The line series.</param>
    /// <returns>A line series presenter.</returns>
    private SeriesPresenter<LineSeries> CreateLineSeriesPresenter(LineSeries series)
    {
        ISeriesView<LineSeries> view = new LineSeriesView();
        return new SeriesPresenter<LineSeries>(view, series, dataSourcePresenterFactory);
    }
}
