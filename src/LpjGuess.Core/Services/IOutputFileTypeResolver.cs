using LpjGuess.Core.Parsers;

namespace LpjGuess.Core.Services;

/// <summary>
/// Resolves output file types from filenames.
/// </summary>
public interface IOutputFileTypeResolver
{
    /// <summary>
    /// Gets the type of an output file from its filename.
    /// </summary>
    /// <param name="filename">Name of the output file.</param>
    /// <returns>The type of the output file.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if BuildLookupTable has not been called or if the file type cannot be resolved.
    /// </exception>
    string GetFileType(string filename);

    /// <summary>
    /// Builds the lookup table for output file types.
    /// </summary>
    void BuildLookupTable(InstructionFileParser parser);
}
