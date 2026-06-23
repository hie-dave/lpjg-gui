using LpjGuess.Core.Models.Graphing.Series;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// Validates a scatter series after its properties or data source change.
/// </summary>
public class ScatterSeriesValidationCommand : SeriesValidationCommand<ScatterSeries>
{
    /// <summary>
    /// Create a new <see cref="ScatterSeriesValidationCommand"/> instance.
    /// </summary>
    /// <param name="series">The series to validate.</param>
    public ScatterSeriesValidationCommand(ScatterSeries series) : base(series)
    {
    }
}
