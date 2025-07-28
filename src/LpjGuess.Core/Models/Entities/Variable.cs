using System.Collections.Generic;

namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a variable in a dataset, containing metadata about the measurements.
/// </summary>
public class Variable
{
    /// <summary>
    /// Unique identifier for this variable.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of this variable.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of this variable.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Units of this variable.
    /// </summary>
    public string Units { get; set; } = string.Empty;
    
    /// <summary>
    /// Aggregation level of this variable.
    /// </summary>
    public AggregationLevel Level { get; set; }
    
    // Navigation properties

    /// <summary>
    /// The dataset to which this variable belongs.
    /// </summary>
    public int DatasetId { get; set; }

    /// <summary>
    /// The dataset to which this variable belongs.
    /// </summary>
    public Dataset Dataset { get; set; } = null!;

    /// <summary>
    /// The layers of this variable.
    /// </summary>
    public ICollection<VariableLayer> Layers { get; set; } = new List<VariableLayer>();
}
