namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a specific layer within a variable (e.g., a specific PFT for LAI).
/// </summary>
public class VariableLayer
{
    /// <summary>
    /// Unique identifier for this layer.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of this layer.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of this layer.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    // Navigation properties

    /// <summary>
    /// The variable to which this layer belongs.
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// The variable to which this layer belongs.
    /// </summary>
    public Variable Variable { get; set; } = null!;
}
