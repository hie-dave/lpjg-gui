using Dave.Benchmarks.Core.Models;

namespace LpjGuess.Core.Models;

/// <summary>
/// Represents a concrete output file stored on disk.
/// </summary>
public class OutputFile
{
    /// <summary>
    /// Output file metadata.
    /// </summary>
    public OutputFileMetadata Metadata { get; private init; }

    /// <summary>
    /// Path to the output file.
    /// </summary>
    public string Path { get; private init; }

    /// <summary>
    /// Create a new <see cref="OutputFile"/> instance.
    /// </summary>
    /// <param name="metadata">Output file metadata.</param>
    /// <param name="path">Path to the output file.</param>
    public OutputFile(OutputFileMetadata metadata, string path)
    {
        Metadata = metadata;
        Path = path;
    }

    /// <inheritdoc/>
    public override string ToString() => Metadata.FileName;
}
