using System.Text;
using Dave.Benchmarks.Core.Models;
using Dave.Benchmarks.Core.Models.Entities;
using Dave.Benchmarks.Core.Models.Importer;
using Dave.Benchmarks.Core.Services;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Classes;
using Microsoft.Extensions.Logging;
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
    public async Task<IEnumerable<SeriesData>> ReadAsync(ModelOutput source)
    {
        return (await Task.WhenAll(
            source.InstructionFiles
                  .Select(f => ReadSimulationAsync(source, GetSimulation(f)))))
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
    /// <returns>The data read from the simulation.</returns>
    private async Task<IEnumerable<SeriesData>> ReadSimulationAsync(
        ModelOutput source,
        Simulation simulation)
    {
        // TODO: async support.
        Quantity quantity = await simulation.ReadOutputFileTypeAsync(source.OutputFileType);

        Layer? layer = quantity.Layers.FirstOrDefault(l => l.Name == source.YAxisColumn);
        if (layer == null)
            // TBI: allow for plotting date on the y-axis.
            throw new InvalidOperationException($"Output {quantity.Name} does not have layer: {source.YAxisColumn}");

        Func<DataPoint, double> xselector = GetSelector(source.XAxisColumn);
        Func<DataPoint, double> yselector = GetSelector(source.YAxisColumn);

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

        return GenerateSeries(
            source,
            simulation,
            xlayer,
            layer,
            xselector,
            yselector);
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
        Func<DataPoint, double>? xselector,
        Func<DataPoint, double>? yselector)
    {
        // First group by context. Each group is a list of data points with the
        // same gridcell, stand, patch, and indiv id (where those properties are
        // applicable for this output file type).
        var xgroups = xlayer.Data.GroupBy(d => GetContext(simulation, d)).ToList();
        var ygroups = ylayer.Data.GroupBy(d => GetContext(simulation, d)).ToList();

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

            string name = GenerateSeriesName(source, xgroup.Key, contexts);

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
    /// <param name="datapoint">The data point.</param>
    /// <returns>The context for the data point.</returns>
    private static SeriesContext GetContext(Simulation simulation, DataPoint datapoint)
    {
        // FIXME: this will throw for coordinates not in the gridlist. Would it
        // be better to use the fallback name (lat, lon) in that case?
        string name = simulation.Gridlist.GetName(datapoint.Longitude, datapoint.Latitude);
        return new SeriesContext(
            new Gridcell(datapoint.Latitude, datapoint.Longitude, name),
            datapoint.Stand,
            datapoint.Patch,
            datapoint.Individual,
            Path.GetFileNameWithoutExtension(simulation.FileName));
    }

    /// <summary>
    /// Generate a series name for the given series context.
    /// </summary>
    /// <param name="source">The model output.</param>
    /// <param name="context">The series context.</param>
    /// <param name="contexts">The list of all series contexts.</param>
    /// <returns>The series name.</returns>
    private static string GenerateSeriesName(ModelOutput source, SeriesContext context, IEnumerable<SeriesContext> contexts)
    {
        // We need to generate a name which will disambiguate each series.
        OutputFileMetadata metadata = OutputFileDefinitions.GetMetadata(source.OutputFileType);
        string name = metadata.Name;

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
            StringBuilder sb = new();
            if (includeStand)
                sb.Append($"s{context.Stand}");
            if (includePatch)
                sb.Append($"p{context.Patch}");
            if (includeIndiv)
                sb.Append($"i{context.Individual}");
            name = $"{name} ({sb})";
        }

        return name;
    }

    /// <summary>
    /// Merge the two layers on all coordinate values.
    /// </summary>
    /// <param name="xlayer">The layer to use for the x-axis.</param>
    /// <param name="ylayer">The layer to use for the y-axis.</param>
    /// <returns>The merged data points.</returns>
    private IEnumerable<OxyPlot.DataPoint> MergeLayers(Layer xlayer, Layer ylayer)
    {
        if (!xlayer.Data.Any() || !ylayer.Data.Any())
            return [];

        DataPoint xfirst = xlayer.Data.First();
        DataPoint yfirst = ylayer.Data.First();

        List<Func<DataPoint, DataPoint, bool>> predicates = [
            (x, y) => x.Timestamp == y.Timestamp,
            (x, y) => x.Latitude == y.Latitude,
            (x, y) => x.Longitude == y.Longitude,
        ];

        if (xfirst.Stand != null || yfirst.Stand != null)
        {
            if (xfirst.Stand == null || yfirst.Stand == null)
                throw new InvalidOperationException("Stand values do not exist in both layers");
            predicates.Add((x, y) => x.Stand == y.Stand);
        }

        if (xfirst.Patch != null || yfirst.Patch != null)
        {
            if (xfirst.Patch == null || yfirst.Patch == null)
                throw new InvalidOperationException("Patch values do not exist in both layers");
            predicates.Add((x, y) => x.Patch == y.Patch);
        }

        if (xfirst.Individual != null || yfirst.Individual != null)
        {
            if (xfirst.Individual == null || yfirst.Individual == null)
                throw new InvalidOperationException("Indiv values do not exist in both layers");
            predicates.Add((x, y) => x.Individual == y.Individual);
        }

        return MergeLayersOn(xlayer, ylayer, predicates);
    }

    /// <summary>
    /// Zip the two layers together matching on the given coordinates.
    /// </summary>
    /// <param name="xlayer">The layer to use for the x-axis.</param>
    /// <param name="ylayer">The layer to use for the y-axis.</param>
    /// <param name="predicates">The matchers to use to match the layers.</param>
    /// <returns>The merged data points.</returns>
    private IEnumerable<OxyPlot.DataPoint> MergeLayersOn(Layer xlayer, Layer ylayer, IReadOnlyList<Func<DataPoint, DataPoint, bool>> predicates)
    {
        return MergeOn(xlayer.Data, ylayer.Data, null, null, predicates.ToArray());
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

    /// <summary>
    /// Convert a model DataPoint to an OxyPlot DataPoint with the date on the
    /// x-axis.
    /// </summary>
    /// <param name="point">The data point to convert.</param>
    /// <returns>An OxyPlot DataPoint requiring a date x-axis.</returns>
    private OxyPlot.DataPoint DataPointToOxyDateDataPoint(DataPoint point)
    {
        return new OxyPlot.DataPoint(DateTimeAxis.ToDouble(point.Timestamp), point.Value);
    }

    /// <inheritdoc />
    public string GetName(ModelOutput source)
    {
        OutputFileMetadata metadata = OutputFileDefinitions.GetMetadata(source.OutputFileType);
        return metadata.Name;
    }
}
