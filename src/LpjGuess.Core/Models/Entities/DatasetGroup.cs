namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a logical grouping of related datasets, typically from the same model run or experiment.
/// </summary>
public class DatasetGroup
{
    /// <summary>
    /// Unique identifier for this group.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of this group.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of this group.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// The time this group was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Indicates whether this group is complete and should not accept new datasets.
    /// </summary>
    public bool IsComplete { get; set; }
    
    /// <summary>
    /// The datasets that belong to this group.
    /// </summary>
    public ICollection<Dataset> Datasets { get; set; } = new List<Dataset>();
    
    /// <summary>
    /// Additional metadata about this group stored as a JSON document.
    /// This can include things like model version, climate scenario, etc.
    /// </summary>
    public string Metadata { get; set; } = "{}";
}
