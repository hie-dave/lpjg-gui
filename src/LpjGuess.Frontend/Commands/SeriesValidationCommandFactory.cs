using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// Factory for creating appropriate validation commands based on series type.
/// </summary>
public class SeriesValidationCommandFactory
{
    /// <summary>
    /// Creates a validation command for the given series.
    /// </summary>
    /// <param name="series">The series to create a validation command for.</param>
    /// <returns>A validation command appropriate for the series type.</returns>
    public ICommand CreateValidationCommand<T>(T series) where T : ISeries
    {
        return series switch
        {
            LineSeries lineSeries => new LineSeriesValidationCommand(lineSeries),
            // Add cases for other series types as needed
            _ => throw new ArgumentException($"Unsupported series type: {series.GetType().Name}")
        };
    }
}
