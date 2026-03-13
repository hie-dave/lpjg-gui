using System.Globalization;
using LpjGuess.Core.Models;
using LpjGuess.Core.Utility;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Core.Parsers;

/// <summary>
/// Parser for LPJ-GUESS gridlist files.
/// </summary>
public class GridlistParser : IGridlistParser
{
    private readonly ILogger<GridlistParser> logger;

    /// <summary>
    /// Create a new <see cref="GridlistParser"/> instance.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public GridlistParser(ILogger<GridlistParser> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc /> 
    public async Task<IEnumerable<Gridcell>> ParseAsync(string gridlist)
    {
        logger.LogInformation("Parsing gridlist file {gridlist}", gridlist);
        using var _ = logger.BeginScope(Path.GetFileName(gridlist));

        string[] lines = await File.ReadAllLinesAsync(gridlist);
        List<Gridcell> coordinates = new();
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            // Skip empty lines.
            if (string.IsNullOrWhiteSpace(line))
                continue;

            coordinates.Add(ParseLine(line, i + 1));
        }
        return coordinates;
    }

    /// <summary>
    /// Parse a single line of the gridlist file, returning a <see cref="Gridcell"/> instance.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <param name="lineNumber">The line number, for error reporting.</param>
    /// <returns>A <see cref="Gridcell"/> instance representing the parsed line.</returns>
    private Gridcell ParseLine(string line, int lineNumber)
    {
        string[] parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            ExceptionHelper.Throw<InvalidDataException>(logger, $"Parser error: line {lineNumber}: lines must contain at least 2 parts (has {parts.Length})");
        if (!double.TryParse(parts[0], CultureInfo.InvariantCulture, out double lon))
            ExceptionHelper.Throw<InvalidDataException>(logger, $"Parser error: line {lineNumber}: failed to parse longitude from '{parts[0]}'");
        if (!double.TryParse(parts[1], CultureInfo.InvariantCulture, out double lat))
            ExceptionHelper.Throw<InvalidDataException>(logger, $"Parser error: line {lineNumber}: failed to parse latitude from '{parts[1]}'");
        if (lon < -180 || lon > 180)
            ExceptionHelper.Throw<InvalidDataException>(logger, $"Parser error: line {lineNumber}: longitude must be in range [-180, 180], but was: {lon}");
        if (lat < -90 || lat > 90)
            ExceptionHelper.Throw<InvalidDataException>(logger, $"Parser error: line {lineNumber}: latitude must be in range [-90, 90], but was: {lat}");

        string? name = parts.Length > 2 ? string.Join(' ', parts.Skip(2)) : null;
        return new Gridcell(lat, lon, name);
    }
}
