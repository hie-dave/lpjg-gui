namespace LpjGuess.Tests;

/// <summary>
/// A temporary directory that is automatically deleted when disposed.
/// </summary>
public class TempDirectory : IDisposable
{
    /// <summary>
    /// The absolute path to the directory.
    /// </summary>
    public string AbsolutePath { get; private init; }

    /// <summary>
    /// Create a temporary directory.
    /// </summary>
    /// <param name="absolutePath">The absolute path to the directory.</param>
    public TempDirectory(string absolutePath)
    {
        AbsolutePath = absolutePath;
        Directory.CreateDirectory(AbsolutePath);
    }

    /// <summary>
    /// Create a temporary directory that will be deleted when disposed.
    /// </summary>
    /// <param name="prefix">The prefix for the directory.</param>
    /// <returns>The temporary directory.</returns>
    public static TempDirectory Create(string? prefix = null)
    {
        string directory = Directory.CreateTempSubdirectory(prefix).FullName;
        return new TempDirectory(directory);
    }

    /// <summary>
    /// Create a directory relative to another directory.
    /// </summary>
    /// <param name="baseDirectory">The base directory.</param>
    /// <param name="fileTree">The file tree to create.</param>
    /// <returns>The temporary directory.</returns>
    public static TempDirectory Relative(TempDirectory baseDirectory, params string[] fileTree)
    {
        string relative = Path.Combine(fileTree);
        string directory = Path.Combine(baseDirectory.AbsolutePath, relative);
        return new TempDirectory(directory);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Directory.Delete(AbsolutePath, true);
    }
}
