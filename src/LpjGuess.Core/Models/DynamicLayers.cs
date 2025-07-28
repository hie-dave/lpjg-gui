namespace LpjGuess.Core.Models;

using LpjGuess.Core.Models.Entities;
using LpjGuess.Core.Models.Importer;

/// <summary>
/// Layer metadata for output files where the layer names are determined at parse-time.
/// </summary>
public class DynamicLayers : ILayerDefinitions
{
    private readonly Unit units;
    private readonly AggregationLevel level;
    private readonly TemporalResolution resolution;
    private readonly HashSet<string> dataLayers;

    /// <summary>
    /// Create a new dynamic layers instance.
    /// </summary>
    /// <param name="units">The units for all data layers.</param>
    /// <param name="level">The level at which data is aggregated.</param>
    /// <param name="resolution">The temporal resolution of the data.</param>
    public DynamicLayers(Unit units, AggregationLevel level, TemporalResolution resolution)
    {
        this.units = units;
        this.level = level;
        this.resolution = resolution;
        this.dataLayers = new HashSet<string>();

        if (resolution == TemporalResolution.Monthly)
            throw new InvalidOperationException("Monthly resolution is not supported for dynamic layers");
    }

    /// <summary>
    /// Add a data layer that was discovered during parsing.
    /// </summary>
    /// <param name="layer">The name of the layer.</param>
    public void AddDataLayer(string layer)
    {
        if (!IsDataLayer(layer))
            throw new InvalidOperationException($"Layer {layer} is a metadata layer");

        dataLayers.Add(layer);
    }

    /// <inheritdoc />
    public Unit GetUnits(string layer)
    {
        if (!IsDataLayer(layer))
            throw new InvalidOperationException($"Layer {layer} is not a data layer");

        return units;
    }

    /// <inheritdoc />
    public bool IsDataLayer(string layer)
    {
        return !ModelConstants.GetMetadataLayers(level, resolution).Contains(layer);
    }
}
