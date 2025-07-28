using LpjGuess.Core.Models.Entities;

namespace LpjGuess.Core.Models;

/// <summary>
/// Metadata about an output file.
/// </summary>
public class OutputFileMetadata
{
    /// <summary>
    /// The file type (e.g. "file_lai").
    /// </summary>
    public string FileName { get; init; }

    /// <summary>
    /// Title of the output file.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Description of the output file.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Layer metadata.
    /// </summary>
    public ILayerDefinitions Layers { get; init; }

    /// <summary>
    /// The level at which data in this file is aggregated.
    /// </summary>
    public AggregationLevel Level { get; init; }

    /// <summary>
    /// The temporal resolution of the data.
    /// </summary>
    public TemporalResolution TemporalResolution { get; init; }

    /// <summary>
    /// Create a new output file metadata instance.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="name">Title of the output file.</param>
    /// <param name="description">Description of the output file.</param>
    /// <param name="layers">Layer metadata.</param>
    /// <param name="level">The level at which data is aggregated.</param>
    /// <param name="resolution">The temporal resolution of the data.</param>
    public OutputFileMetadata(
        string fileName,
        string name,
        string description,
        ILayerDefinitions layers,
        AggregationLevel level,
        TemporalResolution resolution)
    {
        FileName = fileName;
        Name = name;
        Description = description;
        Layers = layers;
        Level = level;
        TemporalResolution = resolution;
    }

    /// <summary>
    /// Gets a display name for the output file which includes the aggregation
    /// level and temporal resolution of the data.
    /// </summary>
    public string GetLongName()
    {
        // Patch-Level Annual LAI.
        return $"{TemporalResolution} {Level}-Level {Name}";
    }

    /// <summary>
    /// Gets a description for the output file which includes the aggregation
    /// level and temporal resolution of the data.
    /// </summary>
    public string GetLongDescription()
    {
        return $"{TemporalResolution} {Level}-Level {Description}";
    }
}
