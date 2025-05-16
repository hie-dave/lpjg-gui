using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Frontend.Interfaces.Presenters;

namespace LpjGuess.Frontend.Interfaces.Factories;

/// <summary>
/// Factory for creating series presenters based on series type.
/// </summary>
public interface ISeriesPresenterFactory
{
    /// <summary>
    /// Creates an appropriate presenter for the given series.
    /// </summary>
    /// <param name="series">The series to create a presenter for.</param>
    /// <returns>A presenter compatible with the series type.</returns>
    ISeriesPresenter CreatePresenter(ISeries series);
}
