using System.Collections.Generic;

namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a Plant Functional Type (PFT) in the model.
/// </summary>
public class Pft
{
    /// <summary>
    /// The unique identifier for this PFT.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of this PFT.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property for individuals of this PFT.
    /// </summary>
    public ICollection<Individual> Individuals { get; set; } = new List<Individual>();
}
