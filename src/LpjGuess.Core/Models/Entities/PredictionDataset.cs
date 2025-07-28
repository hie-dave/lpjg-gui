using LpjGuess.Core.Utility;

namespace LpjGuess.Core.Models.Entities;

/// <summary>
/// Represents a model prediction dataset. Multiple related predictions (e.g. from different sites
/// or climate scenarios) should be grouped using DatasetGroup.
/// </summary>
public class PredictionDataset : Dataset
{
    /// <summary>
    /// The version/commit of the model used to generate this prediction.
    /// </summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>
    /// The climate dataset used as input for this prediction.
    /// </summary>
    public string ClimateDataset { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata about this prediction stored as a JSON document.
    /// This can include things like site characteristics, climate scenario details, etc.
    /// </summary>
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Git patches representing the model changes used to generate this prediction.
    /// </summary>
    public byte[] Patches { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Set the patches for this dataset.
    /// </summary>
    /// <param name="patches">The patches to set.</param>
    public void SetPatches(string patches) => 
        Patches = CompressionUtility.CompressText(patches);
    
    /// <summary>
    /// Get the patches for this dataset.
    /// </summary>
    /// <returns>The patches for this dataset.</returns>
    public string GetPatches() => 
        CompressionUtility.DecompressToText(Patches);
}
