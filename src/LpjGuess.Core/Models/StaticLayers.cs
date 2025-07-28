namespace LpjGuess.Core.Models;

using LpjGuess.Core.Models.Entities;
using LpjGuess.Core.Models.Importer;

/// <summary>
/// Layer metadata for output files with a static set of layers known at compile-time.
/// </summary>
public class StaticLayers : ILayerDefinitions
{
    private readonly IReadOnlyDictionary<string, Unit> layers;
    private readonly AggregationLevel level;
    private readonly TemporalResolution resolution;

    /// <summary>
    /// Create a new static layers instance.
    /// </summary>
    /// <param name="layers">A dictionary of layer name to units for each layer in the output file.</param>
    /// <param name="level">The level at which data is aggregated.</param>
    /// <param name="resolution">The temporal resolution of the data.</param>
    public StaticLayers(IReadOnlyDictionary<string, Unit> layers, AggregationLevel level, TemporalResolution resolution)
    {
        this.layers = layers;
        this.level = level;
        this.resolution = resolution;
    }

    /// <summary>
    /// Create a new static layers instance where all layers have the same units.
    /// </summary>
    /// <param name="layers">A list of layer names.</param>
    /// <param name="units">The units for all layers.</param>
    /// <param name="level">The level at which data is aggregated.</param>
    /// <param name="resolution">The temporal resolution of the data.</param>
    public StaticLayers(IEnumerable<string> layers, Unit units, AggregationLevel level, TemporalResolution resolution)
        : this(layers.ToDictionary(l => l, _ => units), level, resolution)
    {
    }

    /// <inheritdoc />
    public Unit GetUnits(string layer)
    {
        if (!IsDataLayer(layer))
            throw new InvalidOperationException($"Layer {layer} is not a data layer");

        if (!layers.TryGetValue(layer, out var units))
            throw new InvalidOperationException($"Layer {layer} is not a data layer or a static layer");

        return units;
    }

    /// <inheritdoc />
    public bool IsDataLayer(string layer)
    {
        return !ModelConstants.GetMetadataLayers(level, resolution).Contains(layer);
    }
}
