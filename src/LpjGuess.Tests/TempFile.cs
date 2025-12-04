namespace LpjGuess.Tests;

/// <summary>
/// A temporary file that is automatically deleted when disposed.
/// </summary>
public class TempFile : IDisposable
{
    /// <summary>
    /// The absolute path to the file.
    /// </summary>
    public string AbsolutePath { get; private init; }

    /// <summary>
    /// Create a temporary file.
    /// </summary>
    /// <param name="absolutePath">The absolute path to the file.</param>
    public TempFile(string absolutePath)
    {
        AbsolutePath = absolutePath;
        using (File.Create(AbsolutePath)) { }
    }

    /// <summary>
    /// Create a file in the specified directory.
    /// </summary>
    /// <param name="directory">The directory to create the file in.</param>
    /// <param name="fileName">The name of the file.</param>
    public TempFile(string directory, string fileName)
        : this(Path.Combine(directory, fileName))
    {
    }

    /// <summary>
    /// Create a temporary file that will be deleted when disposed.
    /// </summary>
    /// <param name="prefix">The prefix for the file.</param>
    /// <param name="ext">The extension for the file.</param>
    /// <returns>The temporary file.</returns>
    public static TempFile Create(string? prefix = null, string? ext = null)
    {
        prefix ??= "LpjGuess.Tests.TempFile";

        if (ext != null && !ext.StartsWith('.'))
            ext = $".{ext}";
        ext ??= ".tmp";

        string fileName = $"{prefix}_{Guid.NewGuid()}{ext}";
        string file = Path.Combine(Path.GetTempPath(), fileName);
        return new TempFile(file);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        File.Delete(AbsolutePath);
    }
}
