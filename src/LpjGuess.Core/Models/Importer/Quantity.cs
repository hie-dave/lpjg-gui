using LpjGuess.Core.Models.Entities;
using System.Text.Json.Serialization;

namespace LpjGuess.Core.Models.Importer;

/// <summary>
/// Represents a quantity that has been measured, such as LAI or NPP.
/// </summary>
public class Quantity
{
    /// <summary>
    /// Name of the quantity.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Description of the quantity.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Data layers in this quantity.
    /// </summary>
    public IReadOnlyList<Layer> Layers { get; }

    /// <summary>
    /// The level at which data in this quantity is aggregated.
    /// </summary>
    public AggregationLevel Level { get; }

    /// <summary>
    /// The temporal resolution of the data.
    /// </summary>
    public TemporalResolution Resolution { get; }

    /// <summary>
    /// For individual-level outputs, maps individual numbers to their PFT names.
    /// Individual numbers are unique within a dataset (model run).
    /// </summary>
    public IReadOnlyDictionary<int, string>? IndividualPfts { get; }

    /// <summary>
    /// Create a new quantity.
    /// </summary>
    /// <param name="name">Name of the quantity.</param>
    /// <param name="description">Description of the quantity.</param>
    /// <param name="layers">Data layers in this quantity.</param>
    /// <param name="level">The level at which data is aggregated.</param>
    /// <param name="resolution">The temporal resolution of the data.</param>
    /// <param name="individualPfts">For individual-level outputs, maps individual numbers to their PFT names.</param>
    [JsonConstructor]
    public Quantity(
        string name,
        string description,
        IReadOnlyList<Layer> layers,
        AggregationLevel level,
        TemporalResolution resolution,
        IReadOnlyDictionary<int, string>? individualPfts = null)
    {
        Name = name;
        Description = description;
        Layers = layers;
        Level = level;
        Resolution = resolution;
        IndividualPfts = individualPfts;
    }

    /// <summary>
    /// Get the layer with the specified name.
    /// </summary>
    /// <param name="name">Name of the layer to get.</param>
    /// <returns>The layer with the specified name, or null if not found.</returns>
    public Layer? GetLayer(string name) => Layers.FirstOrDefault(l => l.Name == name);
}
