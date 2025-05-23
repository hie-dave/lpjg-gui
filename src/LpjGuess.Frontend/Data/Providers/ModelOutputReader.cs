using Dave.Benchmarks.Core.Models;
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
        return await Task.WhenAll(
            source.InstructionFiles
                  .Select(f => ReadSimulationAsync(source, GetSimulation(f))));
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
    private async Task<SeriesData> ReadSimulationAsync(
        ModelOutput source,
        Simulation simulation)
    {
        // TODO: async support.
        Quantity quantity = await simulation.ReadOutputFileTypeAsync(source.OutputFileType);

        Layer? layer = quantity.Layers.FirstOrDefault(l => l.Name == source.YAxisColumn);
        if (layer == null)
            throw new InvalidOperationException($"Output {quantity.Name} does not have layer: {source.YAxisColumn}");

        string name = GenerateSeriesName(simulation, layer);

        Layer? xlayer = quantity.Layers.FirstOrDefault(l => l.Name == source.XAxisColumn);
        if (xlayer != null)
            return new SeriesData(name, MergeLayers(xlayer, layer));

        if (source.XAxisColumn != "Date")
            throw new NotImplementedException("TBI: Only date is supported on x axis for plots of model outputs.");

        return new SeriesData(name, layer.Data.Select(DataPointToOxyDateDataPoint));
    }

    /// <summary>
    /// Generate a series name for the given simulation and layer.
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    /// <param name="layer">The layer.</param>
    /// <returns>The series name.</returns>
    private static string GenerateSeriesName(Simulation simulation, Layer layer)
    {
        string simulationName = Path.GetFileNameWithoutExtension(simulation.FileName);
        return $"{simulationName}: {layer.Name}";
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
        // For each data point in xlayer, select a data point in ylayer for which
        // all coordinates match.
        foreach (DataPoint xpoint in xlayer.Data)
        {
            // Allow for multiple matches.
            IEnumerable<DataPoint> ypoints = ylayer.Data.Where(y => predicates.All(p => p(xpoint, y)));
            foreach (DataPoint ypoint in ypoints)
                yield return new OxyPlot.DataPoint(xpoint.Value, ypoint.Value);
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
