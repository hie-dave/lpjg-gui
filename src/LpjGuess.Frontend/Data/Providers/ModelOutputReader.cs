using LpjGuess.Core.Extensions;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Entities;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identifiers;
using LpjGuess.Core.Models.Importer;
using LpjGuess.Core.Services;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.DependencyInjection;
using Microsoft.Extensions.Logging;
using OxyPlot.Axes;

using OxyDataPoint = OxyPlot.DataPoint;

namespace LpjGuess.Frontend.Data.Providers;

/// <summary>
/// A data provider for model output files.
/// </summary>
public class ModelOutputReader : IDataProvider<ModelOutput>
{
    /// <summary>
    /// List of simulation readers, which are cached between uses of this class,
    /// to avoid double-parsing.
    /// </summary>
    private static readonly List<SimulationReader> readers = new List<SimulationReader>();

    /// <summary>
    /// The instruction files provider.
    /// </summary>
    private readonly IInstructionFilesProvider insFilesProvider;

    /// <summary>
    /// The experiment provider.
    /// </summary>
    private readonly IExperimentProvider experimentProvider;

    /// <summary>
    /// The logger factory.
    /// </summary>
    private readonly ILoggerFactory loggerFactory;

    /// <summary>
    /// Create a new <see cref="ModelOutputReader"/> instance.
    /// </summary>
    /// <param name="insFilesProvider">The instruction files provider.</param>
    /// <param name="experimentProvider">The experiment provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public ModelOutputReader(
        IInstructionFilesProvider insFilesProvider,
        IExperimentProvider experimentProvider,
        ILoggerFactory loggerFactory)
    {
        this.insFilesProvider = insFilesProvider;
        this.experimentProvider = experimentProvider;
        this.loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SeriesData>> ReadAsync(ModelOutput source, CancellationToken ct)
    {
        // The filtered list of instruction files to be read.
        IEnumerable<InstructionFile> instructionFiles = insFilesProvider.GetGeneratedInstructionFiles();
        return (await Task.WhenAll(
            instructionFiles
                  .Select(f => ReadSimulationAsync(source, GetSimulation(f), ct))))
            .SelectMany(x => x);
    }

    /// <summary>
    /// Get a simulation object for the given instruction file.
    /// </summary>
    /// <param name="instructionFile">The instruction file.</param>
    /// <returns>The simulation reader.</returns>
    public SimulationReader GetSimulation(InstructionFile instructionFile)
    {
        var simulation = readers.FirstOrDefault(s => s.InsFile == instructionFile);
        if (simulation == null)
        {
            simulation = new SimulationReader(instructionFile, loggerFactory);
            lock (readers)
                readers.Add(simulation);
        }

        return simulation;
    }

    /// <inheritdoc />
    public int GetNumSeries(ModelOutput source)
    {
        return insFilesProvider.GetGeneratedInstructionFiles().Count() * source.YAxisColumns.Count();
    }

    /// <summary>
    /// Get the set of valid series identities for the given strategy.
    /// </summary>
    /// <param name="model">The model output.</param>
    /// <param name="strategy">The strategy to get identities for.</param>
    /// <returns>The set of valid series identities.</returns>
    public IEnumerable<SeriesIdentityBase> GetIdentities(ModelOutput model, StyleVariationStrategy strategy)
    {
        ISeriesIdentifier identifier = strategy.CreateIdentifier();
        return identifier switch
        {
            ExperimentIdentifier e => GetIdentities(e),
            SimulationIdentifier s => GetIdentities(s),
            SeriesIdentifier se => GetIdentities(se),
            LayerIdentifier l => GetIdentities(l, model),
            GridcellIdentifier g => GetIdentities(g),
            StandIdentifier s => GetIdentities(s),
            PatchIdentifier p => GetIdentities(p),
            IndividualIdentifier i => GetIdentities(i, model),
            PftIdentifier pft => GetIdentities(pft),
            _ => throw new ArgumentException($"Unknown strategy: {strategy}"),
        };
    }

    /// <summary>
    /// Get the identities of all known PFTs.
    /// </summary>
    /// <param name="identifier">The PFT identifier.</param>
    /// <returns>The set of valid PFT identities.</returns>
    private IEnumerable<SeriesIdentityBase> GetIdentities(PftIdentifier identifier)
    {
        return insFilesProvider.GetGeneratedInstructionFiles()
            .SelectMany(f => GetSimulation(f).Helper.GetEnabledPfts())
            .Select(identifier.Identify)
            .Distinct();
    }

    /// <summary>
    /// Get the identities of all known patches.
    /// </summary>
    /// <param name="identifier">The patch identifier.</param>
    /// <returns>The set of valid patch identities.</returns>
    private IEnumerable<SeriesIdentityBase> GetIdentities(PatchIdentifier identifier)
    {
        int maxNumPatches = insFilesProvider.GetGeneratedInstructionFiles()
            .Select(f => GetSimulation(f).Helper.GetNumPatches())
            .Max();
        return Enumerable.Range(0, maxNumPatches)
            .Select(identifier.Identify);
    }

    /// <summary>
    /// Get the identities of all known stands.
    /// </summary>
    /// <param name="identifier">The stand identifier.</param>
    /// <returns>The set of valid stand identities.</returns>
    private IEnumerable<SeriesIdentityBase> GetIdentities(StandIdentifier identifier)
    {
        int maxNumStands = insFilesProvider.GetGeneratedInstructionFiles()
            .Select(f => GetSimulation(f).Helper.GetEnabledStands().Count())
            .Max();
        return Enumerable.Range(0, maxNumStands)
            .Select(identifier.Identify);
    }

    /// <summary>
    /// Get the identities of all known gridcells.
    /// </summary>
    /// <param name="identifier">The gridcell identifier.</param>
    /// <returns>The set of valid gridcell identities.</returns>
    private IEnumerable<SeriesIdentityBase> GetIdentities(GridcellIdentifier identifier)
    {
        // Technically, the generated simulations could have different
        // gridcells, even though this is probably very rare in practice.
        return insFilesProvider.GetGeneratedInstructionFiles()
            .SelectMany(f => GetSimulation(f).Gridlist.Gridcells)
            .Select(identifier.Identify)
            .Distinct();
    }

    /// <summary>
    /// Get the identities of all known individuals.
    /// </summary>
    /// <param name="identifier">The individual identifier.</param>
    /// <param name="model">The model output.</param>
    /// <returns>The set of valid individual identities.</returns>
    private IEnumerable<SeriesIdentityBase> GetIdentities(IndividualIdentifier identifier, ModelOutput model)
    {
        HashSet<SeriesIdentityBase> identities = [];
        foreach (InstructionFile instructionFile in insFilesProvider.GetGeneratedInstructionFiles())
        {
            SimulationReader reader = GetSimulation(instructionFile);
            var task = reader.ReadOutputFileAsync(model.OutputFileType, CancellationToken.None);
            task.Wait();
            Quantity quantity = task.Result;
            Layer? xlayer = quantity.Layers.FirstOrDefault(l => l.Name == model.XAxisColumn);
            if (xlayer is null)
                throw new InvalidOperationException($"Layer {model.XAxisColumn} not found in output file {model.OutputFileType}");
            // X and Y layers, from the same file, will always have the same
            // indiv values.
            identities.AddRange(xlayer.Data
                .Select(dp => dp.Individual)
                .Where(i => i.HasValue)
                .Select(i => identifier.Identify(i!.Value))
                .Distinct());
        }
        return identities;
    }

    /// <summary>
    /// Get the identities of all known layers.
    /// </summary>
    /// <param name="identifier">The layer identifier.</param>
    /// <param name="model">The model output.</param>
    /// <returns>The set of valid layer identities.</returns>
    private IEnumerable<SeriesIdentityBase> GetIdentities(LayerIdentifier identifier, ModelOutput model)
    {
        List<SeriesIdentityBase> identities = [];
        foreach (InstructionFile instructionFile in insFilesProvider.GetGeneratedInstructionFiles())
        {
            SimulationReader reader = GetSimulation(instructionFile);
            var task = reader.ReadOutputFileMetadataAsync(model.OutputFileType, CancellationToken.None);
            task.Wait();
            foreach (LayerMetadata layer in task.Result)
                identities.Add(identifier.Identify(layer.Name));
        }
        return identities.Distinct();
    }

    /// <summary>
    /// Get the identities of all known simulations.
    /// </summary>
    /// <param name="identifier">The simulation identifier.</param>
    /// <returns>The set of valid simulation identities.</returns>
    private IEnumerable<SeriesIdentityBase> GetIdentities(SimulationIdentifier identifier)
    {
        return insFilesProvider.GetGeneratedInstructionFiles()
            .Select(f => f.SimulationName)
            .Select(identifier.Identify)
            .Distinct();
    }

    /// <summary>
    /// Get the identities of all known experiments.
    /// </summary>
    /// <param name="identifier">The experiment identifier.</param>
    /// <returns>The set of valid experiment identities.</returns>
    private IEnumerable<SeriesIdentityBase> GetIdentities(ExperimentIdentifier identifier)
    {
        return experimentProvider.GetExperiments().Select(identifier.Identify);
    }

    private IEnumerable<SeriesIdentityBase> GetIdentities(SeriesIdentifier identifier)
    {
        // Unfortunately, there is no way to get the series identities from the
        // model output without doing a full parse.
        // TODO: look into caching
        throw new NotImplementedException();
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
        SimulationReader simulation,
        CancellationToken ct)
    {
        // This is a bit clunky, but it should work, and will avoid us parsing
        // unwanted output files.
        SeriesContext proxyContext = new(
            simulation.InsFile.ExperimentName,
            simulation.InsFile.SimulationName,
            new Gridcell(0, 0, ""), // FIXME - this will match against valid gridcells
            "",
            -1,
            -1,
            -1,
            "");
        if (source.Filters.Any(f => f.IsFiltered(proxyContext)))
            return [];

        // Read the output file.
        Quantity quantity = await simulation.ReadOutputFileTypeAsync(source.OutputFileType, ct);
        List<SeriesData> data = new List<SeriesData>();

        // Iterate through the requested layers and generate series for each
        // one. Each layer can in principle generate multiple series - e.g. a
        // patch-level model output will generate one series per patch.
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
        SimulationReader simulation,
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
            SeriesContext context = xgroup.Key;

            // Ignore this series if it is filtered.
            if (source.Filters.Any(f => f.IsFiltered(context)))
                continue;

            IGrouping<SeriesContext, DataPoint>? ygroup = ygroups
                .FirstOrDefault(g => g.Key.Equals(context));

            if (ygroup == null)
            {
                // TODO: log warning
                // This should probably never happen, especially if both layers
                // are coming from the same output file.
                continue;
            }

            string name = GenerateSeriesName(source, context, contexts, ylayer);

            IEnumerable<OxyDataPoint> data = MergeOn(
                xgroup,
                ygroup,
                xselector,
                yselector,
                predicates: (x, y) => x.Timestamp == y.Timestamp).ToList();
            yield return new SeriesData(name, context, data);
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
    private static SeriesContext GetContext(SimulationReader simulation, Layer layer, DataPoint datapoint, IReadOnlyDictionary<int, string>? pftMappings)
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
            simulation.InsFile.ExperimentName,
            simulation.InsFile.SimulationName,
            new Gridcell(datapoint.Latitude, datapoint.Longitude, name),
            layer.Name,
            datapoint.Stand,
            datapoint.Patch,
            datapoint.Individual,
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
    private string GenerateSeriesName(ModelOutput source, SeriesContext context, IEnumerable<SeriesContext> contexts, Layer ylayer)
    {
        // We need to generate a name which will disambiguate each series.
        OutputFileMetadata metadata = OutputFileDefinitions.GetMetadata(source.OutputFileType);

        // TODO: do we need metadata name if all series on the plot use the same
        // data source (and therefore the same output file type)?
        string name = metadata.Name;
        if (source.YAxisColumns.Count() > 1)
            name = ylayer.Name;

        // Gridcell name should be included if there are multiple gridcells.
        bool multiGridcells = GetIdentities(source, StyleVariationStrategy.ByGridcell).Count() > 1;
        if (metadata.Level >= AggregationLevel.Gridcell && multiGridcells)
        {
            // Gridcell.Name will be (lat,lon) if the gridcell is unnamed.
            name = $"{context.Gridcell.Name}: {name}";
        }

        // Simulation name should be included if there are multiple simulations.
        if (insFilesProvider.GetGeneratedInstructionFiles().Count() > 1)
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
    private static IEnumerable<OxyDataPoint> MergeOn(
        IEnumerable<DataPoint> xpoints,
        IEnumerable<DataPoint> ypoints,
        Func<DataPoint, double>? xselector = null,
        Func<DataPoint, double>? yselector = null,
        params Func<DataPoint, DataPoint, bool>[] predicates)
    {
        xselector ??= x => x.Value;
        yselector ??= y => y.Value;

        // For each data point in xlayer, select a data point in ylayer for which
        // all coordinates match.
        foreach (DataPoint x in xpoints)
        {
            // Allow for multiple matches.
            IEnumerable<DataPoint> matches = ypoints
                .Where(yi => predicates.All(p => p(x, yi)));
            foreach (DataPoint y in matches)
                yield return new OxyDataPoint(xselector(x), yselector(y));
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
