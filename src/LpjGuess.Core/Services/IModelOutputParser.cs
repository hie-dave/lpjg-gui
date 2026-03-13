using LpjGuess.Core.Models.Importer;

namespace LpjGuess.Core.Services;

/// <summary>
/// Interface to a class that can parse model output files and extract
/// quantities and metadata.
/// </summary>
public interface IModelOutputParser
{
    /// <summary>
    /// Parses an output file.
    /// </summary>
    /// <param name="filePath">Path to the output file.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the parse operation.</returns>
    Task<Quantity> ParseOutputFileAsync(string filePath, CancellationToken ct = default);

    /// <summary>
    /// Parse the header row of the specified output file.
    /// TODO: refactor the main parse method to call this one. At the moment, we
    /// have duplicated logic.
    /// </summary>
    /// <param name="filePath">Path to the file to be read.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>Collection of layer metadata, if any is found.</returns>
    Task<IEnumerable<LayerMetadata>> ParseOutputFileHeaderAsync(string filePath, CancellationToken ct);
}
