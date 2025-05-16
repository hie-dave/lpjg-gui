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
    public IEnumerable<SeriesData> Read(ModelOutput source)
    {
        return source.InstructionFiles.Select(f => ReadSimulation(source, GetSimulation(f)));
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
    private SeriesData ReadSimulation(
        ModelOutput source,
        Simulation simulation)
    {
        // TODO: async support.
        Task<Quantity> task = simulation.ReadOutputFileTypeAsync(source.OutputFileType);
        task.Wait();

        Quantity quantity = task.Result;
        Layer? layer = quantity.Layers.FirstOrDefault(l => l.Name == source.YAxisColumn);
        if (layer == null)
            throw new InvalidOperationException($"Output {quantity.Name} does not have layer: {source.YAxisColumn}");

        if (source.XAxisColumn != "Date")
            throw new NotImplementedException("TBI: Only date is supported on x axis for plots of model outputs.");

        return new SeriesData(layer.Name, layer.Data.Select(DataPointToOxyPlot));
    }

    /// <summary>
    /// Convert a model DataPoint to an OxyPlot DataPoint with the date on the
    /// x-axis.
    /// </summary>
    /// <param name="point">The data point to convert.</param>
    /// <returns>An OxyPlot DataPoint requiring a date x-axis.</returns>
    private OxyPlot.DataPoint DataPointToOxyPlot(DataPoint point)
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
