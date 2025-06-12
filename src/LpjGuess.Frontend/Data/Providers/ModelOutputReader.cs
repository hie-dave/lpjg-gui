using Dave.Benchmarks.Core.Models;
using Dave.Benchmarks.Core.Models.Entities;
using Dave.Benchmarks.Core.Models.Importer;
using Dave.Benchmarks.Core.Services;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Classes;
using OxyPlot.Axes;

namespace LpjGuess.Frontend.Data.Providers;

/// <summary>
/// A data provider for model output files.
/// </summary>
public class ModelOutputReader : IDataProvider<ModelOutput>
{
    /// <summary>
    /// List of simulation objects, which are cached between uses of this class,
    /// to avoid double-parsing.
    /// </summary>
    private static readonly List<Simulation> simulations = new List<Simulation>();

    /// <summary>
    /// Create a new <see cref="ModelOutputReader"/> instance.
    /// </summary>
    public ModelOutputReader()
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SeriesData>> ReadAsync(ModelOutput source, CancellationToken ct)
    {
        return (await Task.WhenAll(
            source.InstructionFiles
                  .Select(f => ReadSimulationAsync(source, GetSimulation(f), ct))))
            .SelectMany(x => x);
    }

    /// <summary>
    /// Get a simulation object for the given instruction file.
    /// </summary>
    /// <param name="instructionFile"></param>
    /// <returns></returns>
    public static Simulation GetSimulation(string instructionFile)
    {
        var simulation = simulations.FirstOrDefault(s => s.FileName == instructionFile);
        if (simulation == null)
        {
            simulation = new Simulation(instructionFile);
            lock (simulations)
                simulations.Add(simulation);
        }

        return simulation;
    }

    /// <summary>
    /// Read data for a single simulation.
    /// </summary>
    /// <param name="source">The model output.</param>
    /// <param name="simulation">The simulation from which to read the data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The data read from the simulation.</returns>
    private async Task<IEnumerable<SeriesData>> ReadSimulationAsync(
        ModelOutput source,
        Simulation simulation,
        CancellationToken ct)
    {
        // TODO: async support.
        Quantity quantity = await simulation.ReadOutputFileTypeAsync(source.OutputFileType, ct);
        List<SeriesData> data = new List<SeriesData>();

        foreach (string column in source.YAxisColumns)
        {
            Layer? layer = quantity.Layers.FirstOrDefault(l => l.Name == column);
            if (layer == null)
                // TBI: allow for plotting date on the y-axis.
                throw new InvalidOperationException($"Output {quantity.Name} does not have layer: {column}");

            Func<DataPoint, double> xselector = GetSelector(source.XAxisColumn);
            Func<DataPoint, double> yselector = GetSelector(column);

            // "Date" is a valid layer name from the user's perspective, but it
            // doesn't correspond to an actual layer object. Rather, it corresponds
            // to the Timestamp property of the data points. Therefore, we can pass
            // in the same layer for both x and y, and the x selector will return
            // DateTimeAxis.ToDouble() for the timestamps.
            Layer? xlayer;
            if (source.XAxisColumn == "Date")
                xlayer = layer;
            else
                xlayer = quantity.Layers.FirstOrDefault(l => l.Name == source.XAxisColumn);
            if (xlayer == null)
                throw new InvalidOperationException($"Unknown layer name: {source.XAxisColumn}");

            data.AddRange(GenerateSeries(
                source,
                simulation,
                xlayer,
                layer,
                quantity.IndividualPfts,
                xselector,
                yselector));
        }
        return data;
    }

    /// <summary>
    /// Get a selector function for the given column.
    /// </summary>
    /// <param name="column">The name of the column to be displayed on the graph.</param>
    /// <returns>Function which takes a data point and returns a numeric value to be displayed on the plot.</returns>
    private static Func<DataPoint, double> GetSelector(string column)
    {
        if (column == "Date")
            return dp => DateTimeAxis.ToDouble(dp.Timestamp);

        return dp => dp.Value;
    }

    private IEnumerable<SeriesData> GenerateSeries(
        ModelOutput source,
        Simulation simulation,
        Layer xlayer,
        Layer ylayer,
        IReadOnlyDictionary<int, string>? pftMappings,
        Func<DataPoint, double>? xselector,
        Func<DataPoint, double>? yselector)
    {
        // First group by context. Each group is a list of data points with the
        // same gridcell, stand, patch, and indiv id (where those properties are
        // applicable for this output file type).
        var xgroups = xlayer.Data.GroupBy(d => GetContext(simulation, xlayer, d, pftMappings)).ToList();
        var ygroups = ylayer.Data.GroupBy(d => GetContext(simulation, ylayer, d, pftMappings)).ToList();

        IEnumerable<SeriesContext> contexts = xgroups.Select(g => g.Key).ToList();

        // Now we can zip the groups together.
        foreach (IGrouping<SeriesContext, DataPoint> xgroup in xgroups)
        {
            IGrouping<SeriesContext, DataPoint>? ygroup = ygroups
                .FirstOrDefault(g => g.Key.Equals(xgroup.Key));

            if (ygroup == null)
            {
                // TODO: log warning
                // This should probably never happen, especially if both layers
                // are coming from the same output file.
                continue;
            }

            string name = GenerateSeriesName(source, xgroup.Key, contexts, ylayer);

            IEnumerable<OxyPlot.DataPoint> data = MergeOn(
                xgroup,
                ygroup,
                xselector,
                yselector,
                predicates: (x, y) => x.Timestamp == y.Timestamp).ToList();
            yield return new SeriesData(name, xgroup.Key, data);
        }
    }

    /// <summary>
    /// Get the context for the given data point.
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    /// <param name="layer">The layer.</param>
    /// <param name="datapoint">The data point.</param>
    /// <param name="pftMappings">The PFT mappings.</param>
    /// <returns>The context for the data point.</returns>
    private static SeriesContext GetContext(Simulation simulation, Layer layer, DataPoint datapoint, IReadOnlyDictionary<int, string>? pftMappings)
    {
        // FIXME: this will throw for coordinates not in the gridlist. Would it
        // be better to use the fallback name (lat, lon) in that case?
        string name = simulation.Gridlist.GetName(datapoint.Longitude, datapoint.Latitude);
        string? pft = null;
        if (datapoint.Individual != null)
        {
            if (pftMappings is null)
                throw new InvalidOperationException($"Individual-level quantity does not have any individual <-> PFT mappings");
            if (!pftMappings.TryGetValue(datapoint.Individual.Value, out string? pftValue))
                throw new InvalidOperationException($"Individual {datapoint.Individual.Value} does not have a PFT mapping ({pftMappings.Count} mappings)");
            pft = pftValue;
        }
        return new SeriesContext(
            new Gridcell(datapoint.Latitude, datapoint.Longitude, name),
            layer.Name,
            datapoint.Stand,
            datapoint.Patch,
            datapoint.Individual,
            Path.GetFileNameWithoutExtension(simulation.FileName),
            pft);
    }

    /// <summary>
    /// Generate a series name for the given series context.
    /// </summary>
    /// <param name="source">The model output.</param>
    /// <param name="context">The series context.</param>
    /// <param name="contexts">The list of all series contexts.</param>
    /// <param name="ylayer">The y-layer.</param>
    /// <returns>The series name.</returns>
    private static string GenerateSeriesName(ModelOutput source, SeriesContext context, IEnumerable<SeriesContext> contexts, Layer ylayer)
    {
        // We need to generate a name which will disambiguate each series.
        OutputFileMetadata metadata = OutputFileDefinitions.GetMetadata(source.OutputFileType);

        // TODO: do we need metadata name if all series on the plot use the same
        // data source (and therefore the same output file type)?
        string name = metadata.Name;
        if (source.YAxisColumns.Count() > 1)
            name = ylayer.Name;

        // Gridcell name should be included if there are multiple gridcells.
        if (metadata.Level >= AggregationLevel.Gridcell && contexts.Select(c => c.Gridcell).Distinct().Count() > 1)
        {
            // Gridcell.Name will be (lat,lon) if the gridcell is unnamed.
            name = $"{context.Gridcell.Name}: {name}";
        }

        // Simulation name should be included if there are multiple simulations.
        if (source.InstructionFiles.Count > 1)
        {
            if (context.SimulationName == null)
                throw new InvalidOperationException("Simulation name is null");
            name = $"{context.SimulationName}: {name}";
        }

        bool includeStand = metadata.Level >= AggregationLevel.Stand && contexts.Select(c => c.Stand).Distinct().Count() > 1;
        bool includePatch = metadata.Level >= AggregationLevel.Patch && contexts.Select(c => c.Patch).Distinct().Count() > 1;
        bool includeIndiv = metadata.Level >= AggregationLevel.Individual && contexts.Select(c => c.Individual).Distinct().Count() > 1;

        if (includeStand || includePatch || includeIndiv)
        {
            List<string> contextSpecifiers = new List<string>();
            if (includeStand)
                contextSpecifiers.Add($"s{context.Stand}");
            if (includePatch)
                contextSpecifiers.Add($"p{context.Patch}");
            if (includeIndiv)
                // Pft should never be null in indiv-level outputs. This will
                // get better once we do the coordinate/context refactor.
                contextSpecifiers.Add(context.Pft!);
            name = $"{name} ({string.Join(", ", contextSpecifiers)})";
        }

        return name;
    }

    /// <summary>
    /// Zip the two collections together matching on the given predicates.
    /// </summary>
    /// <param name="xpoints">Collection of x values.</param>
    /// <param name="ypoints">Collection of y values.</param>
    /// <param name="predicates">The matchers to use to match the collections.</param>
    /// <param name="xselector">Selector for the x value. If null, x.Value will be selected.</param>
    /// <param name="yselector">Selector for the y value. If null, y.Value will be selected.</param>
    /// <returns>The merged data points.</returns>
    private IEnumerable<OxyPlot.DataPoint> MergeOn(
        IEnumerable<DataPoint> xpoints,
        IEnumerable<DataPoint> ypoints,
        Func<DataPoint, double>? xselector = null,
        Func<DataPoint, double>? yselector = null,
        params Func<DataPoint, DataPoint, bool>[] predicates)
    {
        if (xselector == null)
            xselector = x => x.Value;
        if (yselector == null)
            yselector = y => y.Value;

        // For each data point in xlayer, select a data point in ylayer for which
        // all coordinates match.
        foreach (DataPoint x in xpoints)
        {
            // Allow for multiple matches.
            IEnumerable<DataPoint> matches = ypoints
                .Where(yi => predicates.All(p => p(x, yi)));
            foreach (DataPoint y in matches)
                yield return new OxyPlot.DataPoint(xselector(x), yselector(y));
        }
        // We can assume commutativity of the predicates, so there's no need for
        // double-iteration.
    }

    /// <inheritdoc />
    public string GetName(ModelOutput source)
    {
        OutputFileMetadata metadata = OutputFileDefinitions.GetMetadata(source.OutputFileType);
        return metadata.Name;
    }
}
