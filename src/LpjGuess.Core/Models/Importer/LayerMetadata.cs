namespace LpjGuess.Core.Models.Importer;

/// <summary>
/// Metadata for a layer (column) in an output file.
/// </summary>
public class LayerMetadata
{
    /// <summary>
    /// Name of the layer.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// Units of the layer.
    /// </summary>
    public Unit Units { get; private init; }

    /// <summary>
    /// Create a new <see cref="LayerMetadata"/> instance.
    /// </summary>
    /// <param name="name">Name of the layer.</param>
    /// <param name="units">Units of the layer.</param>
    public LayerMetadata(string name, Unit units)
    {
        Name = name;
        Units = units;
    }
}
