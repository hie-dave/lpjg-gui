using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Importer;
using LpjGuess.Core.Utility;

using Microsoft.Extensions.Logging;

using TemporalResolution = LpjGuess.Core.Models.Entities.TemporalResolution;
using AggregationLevel = LpjGuess.Core.Models.Entities.AggregationLevel;
using System.Text;

namespace LpjGuess.Core.Services;

/// <summary>
/// Parser for LPJ-GUESS model output files.
/// </summary>
public class ModelOutputParser
{
    private readonly ILogger logger;
    private readonly IOutputFileTypeResolver resolver;

    /// <summary>
    /// Creates a new instance of the ModelOutputParser.
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages.</param>
    /// <param name="resolver">Resolver for output file types.</param>
    public ModelOutputParser(ILogger<ModelOutputParser> logger, IOutputFileTypeResolver resolver)
    {
        this.logger = logger;
        this.resolver = resolver;
    }

    /// <summary>
    /// Parses an output file.
    /// </summary>
    /// <param name="filePath">Path to the output file.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the parse operation.</returns>
    public async Task<Quantity> ParseOutputFileAsync(
        string filePath,
        CancellationToken ct = default)
    {
        logger.LogDebug("Parsing output file: {filePath}", filePath);

        try
        {
            return await ParseOutputFileInternalAsync(filePath, ct);
        }
        catch (Exception ex) when (ex is not InvalidDataException && ex is not TaskCanceledException)
        {
            // Log and rethrow unexpected exceptions
            // InvalidDataException is already logged by ExceptionHelper
            logger.LogError(ex, "Failed to parse output file: {filePath} - {ex.Message}", filePath, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Parse the header row of the specified output file.
    /// TODO: refactor the main parse method to call this one. At the moment, we
    /// have duplicated logic.
    /// </summary>
    /// <param name="filePath">Path to the file to be read.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>Collection of layer metadata, if any is found.</returns>
    public async Task<IEnumerable<LayerMetadata>> ParseOutputFileHeaderAsync(string filePath, CancellationToken ct)
    {
        logger.LogDebug("Retrieving output file type");
        string fileName = Path.GetFileName(filePath);
        string fileType = resolver.GetFileType(fileName);
        logger.LogTrace("Output file {fileName} successfully resolved to type: {fileType}", fileName, fileType);

        logger.LogDebug("Retrieving output file metadata");
        OutputFileMetadata metadata = OutputFileDefinitions.GetMetadata(fileType);
        logger.LogTrace("Output file metadata successfully retrieved: {description}", metadata.Description);

        logger.LogDebug("Reading output file");
        string? line = await ReadLineAsync(filePath, ct);
        if (line == null)
            ExceptionHelper.Throw<InvalidDataException>(logger, "File must contain at least a header row and one data row");
        var state = new ParserState(filePath);

        // Parse header row to get column indices.
        logger.LogDebug("Parsing header row");
        string[] headers = SplitLine(line);
        return headers
            .Where(metadata.Layers.IsDataLayer)
            .Select(h => new LayerMetadata(h, metadata.Layers.GetUnits(h)));
    }

    /// <summary>
    /// Read a single line from the specified file.
    /// </summary>
    /// <param name="file">Path to the file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The first line of the file, or null if the file is empty.</returns>
    private async Task<string?> ReadLineAsync(string file, CancellationToken ct)
    {
        using (StreamReader reader = new StreamReader(file))
            return await reader.ReadLineAsync(ct);
    }

    private async Task<Quantity> ParseOutputFileInternalAsync(
        string filePath,
        CancellationToken ct)
    {
        logger.LogDebug("Retrieving output file type");
        string fileName = Path.GetFileName(filePath);
        string fileType = resolver.GetFileType(fileName);
        logger.LogTrace("Output file {fileName} successfully resolved to type: {fileType}", fileName, fileType);

        logger.LogDebug("Retrieving output file metadata");
        OutputFileMetadata metadata = OutputFileDefinitions.GetMetadata(fileType);
        logger.LogTrace("Output file metadata successfully retrieved: {description}", metadata.Description);

        logger.LogDebug("Reading output file");
        string[] lines = await File.ReadAllLinesAsync(filePath, ct);
        if (lines.Length < 2)
            ExceptionHelper.Throw<InvalidDataException>(logger, "File must contain at least a header row and one data row");
        var state = new ParserState(filePath);

        // Parse header row to get column indices.
        logger.LogDebug("Parsing header row");
        string[] headers = SplitLine(lines[0]);
        IReadOnlyDictionary<string, int> indices = GetColumnIndices(headers);
        string[] dataColumns = headers.Where(metadata.Layers.IsDataLayer).ToArray();

        // Create a dictionary to hold data points for each series
        Dictionary<string, List<DataPoint>> seriesData = [];

        // Skip required columns
        foreach (string name in dataColumns)
            seriesData[name] = new List<DataPoint>();

        // Parse data rows.
        for (int i = 1; i < lines.Length; i++)
        {
            state.CurrentLine = i;
            string[] values = SplitLine(lines[i]);
            using var __ = logger.BeginScope("Row {i}", i);

            if (values.Length != headers.Length)
                ExceptionHelper.Throw<InvalidDataException>(logger, $"Invalid number of columns: has {values.Length} columns but header has {headers.Length}");

            // Parse required columns.
            Coordinate point = ParseRequiredColumns(values, indices, metadata);

            // Special handling for PFT column in individual outputs
            if (metadata.Level == AggregationLevel.Individual && headers.Contains(ModelConstants.PftLayer))
            {
                if (point.Individual is null)
                    ExceptionHelper.Throw<InvalidDataException>(logger, "Individual ID is required for individual-level outputs");

                string pftName = values[indices[ModelConstants.PftLayer]];
                int indivId = point.Individual!.Value;

                // Check if we've seen this individual before
                if (state.IndividualPfts.TryGetValue(indivId, out var existing))
                {
                    if (existing.PftName != pftName)
                    {
                        ExceptionHelper.Throw<InvalidDataException>(logger,
                            $"Inconsistent PFT mapping in file {Path.GetFileName(state.FilePath)}: " +
                            $"Individual {indivId} is mapped to '{pftName}' on line {i + 1} " +
                            $"but was mapped to '{existing.PftName}' on line {existing.LineNumber + 1}");
                    }
                }
                else
                {
                    // First time seeing this individual
                    state.IndividualPfts[indivId] = (pftName, i);
                }
            }

            // Parse data values for each series.
            logger.LogTrace("Parsing data values");
            foreach (string name in dataColumns)
            {
                if (!double.TryParse(values[indices[name]], out double value))
                    ExceptionHelper.Throw<InvalidDataException>(logger, $"Invalid value: failed to parse double: {values[indices[name]]}");

                // fixme
                DateTime timestamp = point.Timestamp;
                if (metadata.TemporalResolution == TemporalResolution.Monthly)
                {
                    int month;
                    try
                    {
                        month = TimeUtils.GetMonth(name);
                    }
                    catch (ArgumentException)
                    {
                        if (metadata.Layers is StaticLayers s && ModelConstants.MonthCols.All(s.IsDataLayer))
                            // File has Jan..Dec plus some additional layers.
                            // E.g. file_mald has Jan..Dec plus MAXALD.
                            month = 12;
                        else
                            throw;
                    }

                    // Last day of month.
                    timestamp = new DateTime(point.Timestamp.Year, month, 1).AddMonths(1).AddDays(-1);
                }

                seriesData[name].Add(new DataPoint(
                    timestamp,
                    point.Lon,
                    point.Lat,
                    value,
                    point.Stand,
                    point.Patch,
                    point.Individual));
            }
        }

        logger.LogDebug("File parsed successfully");

        // Add series to quantity.
        List<Layer> layers = [];
        foreach ((string name, List<DataPoint> points) in seriesData)
            layers.Add(new Layer(name, metadata.Layers.GetUnits(name), points));

        // Convert ParserState's IndividualPfts to simple dictionary for the Quantity
        var individualPfts = metadata.Level == AggregationLevel.Individual
            ? state.IndividualPfts.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.PftName)
            : null;

        return new Quantity(
            metadata.Name,
            metadata.Description,
            layers,
            metadata.Level,
            metadata.TemporalResolution,
            individualPfts);
    }

    /// <summary>
    /// Split a line of text into individual columns, without parsing any values.
    /// </summary>
    /// <param name="line">The line of text to split.</param>
    /// <returns>An array of columns.</returns>
    private string[] SplitLine(string line)
    {
        return line.Split(['\t', ' '], StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Get column indices from a header row.
    /// </summary>
    /// <param name="headers">The headers of the file.</param>
    /// <returns>A dictionary mapping column names to column indices.</returns>
    private IReadOnlyDictionary<string, int> GetColumnIndices(string[] headers)
    {
        Dictionary<string, int> indices = new();
        for (int i = 0; i < headers.Length; i++)
            indices[headers[i]] = i;
        return indices;
    }

    /// <summary>
    /// Represents a point in space and time.
    /// </summary>
    /// <param name="Lon">Longitude in degrees.</param>
    /// <param name="Lat">Latitude in degrees.</param>
    /// <param name="Timestamp">Date/Time of the data point.</param>
    /// <param name="Stand">Stand ID, if applicable.</param>
    /// <param name="Patch">Patch ID, if applicable.</param>
    /// <param name="Individual">Individual ID, if applicable.</param>
    private record Coordinate(
        double Lon,
        double Lat,
        DateTime Timestamp,
        int? Stand = null,
        int? Patch = null,
        int? Individual = null);

    /// <summary>
    /// Tracks state while parsing a file, including PFT mappings and line numbers.
    /// </summary>
    private class ParserState
    {
        /// <summary>
        /// Maps individual IDs to their PFT name and the line where first encountered.
        /// </summary>
        public Dictionary<int, (string PftName, int LineNumber)> IndividualPfts { get; } = [];

        /// <summary>
        /// The current line being parsed (0-based).
        /// </summary>
        public int CurrentLine { get; set; }

        /// <summary>
        /// The path to the file being parsed.
        /// </summary>
        public string FilePath { get; }

        public ParserState(string filePath)
        {
            FilePath = filePath;
        }
    }

    /// <summary>
    /// Parse the required columns from a row of data.
    /// </summary>
    /// <param name="values">The raw values from the row.</param>
    /// <param name="indices">The column indices for the required columns.</param>
    /// <param name="metadata">Metadata about the output file.</param>
    /// <returns>A coordinate representing the location of the data point.</returns>
    /// <exception cref="InvalidDataException">Thrown if the required columns cannot be parsed or contain invalid values.</exception>
    private Coordinate ParseRequiredColumns(string[] values, IReadOnlyDictionary<string, int> indices, OutputFileMetadata metadata)
    {
        if (!indices.TryGetValue("Lon", out int lonIndex))
            ExceptionHelper.Throw<InvalidDataException>(logger, "Missing required column: Lon");
        if (!indices.TryGetValue("Lat", out int latIndex))
            ExceptionHelper.Throw<InvalidDataException>(logger, "Missing required column: Lat");

        if (!double.TryParse(values[lonIndex], out double lon))
            ExceptionHelper.Throw<InvalidDataException>(logger, $"Invalid longitude value: {values[lonIndex]}");
        if (!double.TryParse(values[latIndex], out double lat))
            ExceptionHelper.Throw<InvalidDataException>(logger, $"Invalid latitude value: {values[latIndex]}");

        if (lon < 0 || lon > 360)
            ExceptionHelper.Throw<InvalidDataException>(logger, $"Invalid longitude value: {values[lonIndex]}");

        // Parse timestamp based on temporal resolution
        DateTime timestamp = ParseTimestamp(values, indices, metadata.TemporalResolution);

        // Parse additional IDs based on aggregation level
        int? standId = null;
        int? patchId = null;
        int? individualId = null;

        if (metadata.Level >= AggregationLevel.Stand)
        {
            // Stand is optional and will not be written if only one stand is
            // present.
            standId = 0;
            if (indices.TryGetValue(ModelConstants.StandLayer, out int standIndex))
            {
                if (!int.TryParse(values[standIndex], out int stand))
                    ExceptionHelper.Throw<InvalidDataException>(logger, $"Invalid stand value: {values[standIndex]}");
                standId = stand;
        }

        if (metadata.Level >= AggregationLevel.Patch)
        {
                if (!indices.TryGetValue(ModelConstants.PatchLayer, out int patchIndex))
                    ExceptionHelper.Throw<InvalidDataException>(logger, "Missing required column: Patch");
                if (!int.TryParse(values[patchIndex], out int patch))
                    ExceptionHelper.Throw<InvalidDataException>(logger, $"Invalid patch value: {values[patchIndex]}");
                patchId = patch;

                if (metadata.Level == AggregationLevel.Individual)
        {
                    if (!indices.TryGetValue(ModelConstants.IndivLayer, out int individualIndex))
                        ExceptionHelper.Throw<InvalidDataException>(logger, "Missing required column: Individual");
                    if (!int.TryParse(values[individualIndex], out int individual))
                        ExceptionHelper.Throw<InvalidDataException>(logger, $"Invalid individual value: {values[individualIndex]}");
                    individualId = individual;
                }
            }
        }

        return new Coordinate(lon, lat, timestamp, standId, patchId, individualId);
    }

    /// <summary>
    /// Parse timestamp from a row based on temporal resolution.
    /// </summary>
    private DateTime ParseTimestamp(string[] values, IReadOnlyDictionary<string, int> indices, TemporalResolution resolution)
    {
        if (!indices.TryGetValue("Year", out int yearIndex))
            ExceptionHelper.Throw<InvalidDataException>(logger, "Missing required column: Year");
        if (!int.TryParse(values[yearIndex], out int year))
            ExceptionHelper.Throw<InvalidDataException>(logger, $"Invalid year value: {values[yearIndex]}");

        if (resolution == TemporalResolution.Annual)
            return new DateTime(year, 1, 1).AddDays(364); // Not always end of year!

        if (resolution == TemporalResolution.Daily || resolution == TemporalResolution.Subdaily)
        {
            if (!indices.TryGetValue("Day", out int dayIndex))
                ExceptionHelper.Throw<InvalidDataException>(logger, "Missing required column: Day");
            if (!int.TryParse(values[dayIndex], out int day))
                ExceptionHelper.Throw<InvalidDataException>(logger, $"Invalid day value: {values[dayIndex]}");

            // Day is 0-indexed.
            return new DateTime(year, 1, 1).AddDays(day);
        }

        if (resolution != TemporalResolution.Monthly)
            ExceptionHelper.Throw<InvalidDataException>(logger, $"Unexpected temporal resolution: {resolution}");

        // Actual date in monthly timestep depends on column.
        return new DateTime(year, 1, 1);
    }
}
