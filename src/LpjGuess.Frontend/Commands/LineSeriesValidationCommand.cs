using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// A validation command for line series that ensures style providers are compatible
/// with the current data source's allowed style variation strategies.
/// </summary>
public class LineSeriesValidationCommand : SeriesValidationCommand<LineSeries>
{
    /// <summary>
    /// Creates a new <see cref="LineSeriesValidationCommand"/> instance.
    /// </summary>
    /// <param name="series">The line series to validate.</param>
    public LineSeriesValidationCommand(LineSeries series) : base(series)
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<ICommand> GenerateValidationCommands()
    {
        List<ICommand> commands = base.GenerateValidationCommands().ToList();

        // Validate and generate commands for each style provider.
        if (!IsStyleProviderValid(s => s.Type))
            commands.Add(GenerateStyleProviderFixer(
                s => s.Type,
                (s, p) => s.Type = p));

        if (!IsStyleProviderValid(s => s.Thickness))
            commands.Add(GenerateStyleProviderFixer(
                s => s.Thickness,
                (s, p) => s.Thickness = p));

        return commands;
    }
}
