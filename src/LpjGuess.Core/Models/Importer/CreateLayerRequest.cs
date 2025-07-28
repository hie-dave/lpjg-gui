namespace LpjGuess.Core.Models.Importer;

/// <summary>
/// Request model for creating a new layer in a variable.
/// </summary>
public class CreateLayerRequest
{
    /// <summary>
    /// Name of the layer.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Description of the layer.
    /// </summary>
    public string Description { get; set; } = null!;
}
