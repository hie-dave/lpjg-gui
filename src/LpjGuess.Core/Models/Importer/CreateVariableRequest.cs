using LpjGuess.Core.Models.Entities;

namespace LpjGuess.Core.Models.Importer;

/// <summary>
/// Request model for creating a new variable.
/// </summary>
public class CreateVariableRequest
{
    /// <summary>
    /// Name of the variable.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Description of the variable.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// The level at which data in this variable is aggregated.
    /// </summary>
    public AggregationLevel Level { get; set; }

    /// <summary>
    /// The units for this variable.
    /// </summary>
    public string Units { get; set; } = null!;

    /// <summary>
    /// For individual-level outputs, maps individual numbers to their PFT names.
    /// Individual numbers are unique within a dataset (model run).
    /// </summary>
    public IReadOnlyDictionary<int, string>? IndividualPfts { get; set; }
}
