using System.Text;
using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Base class for path resolvers.
/// </summary>
public abstract class PathResolverBase : IPathResolver
{
    /// <summary>
    /// The base output directory.
    /// </summary>
    private readonly string outputDirectory;

    /// <summary>
    /// The strategy used to generate simulation names.
    /// </summary>
    private readonly ISimulationNamingStrategy namingStrategy;

    /// <summary>
    /// Create a new <see cref="PathResolverBase"/> instance.
    /// </summary>
    /// <param name="outputDirectory">The base output directory.</param>
    /// <param name="namingStrategy">The naming strategy.</param>
    public PathResolverBase(string outputDirectory, ISimulationNamingStrategy namingStrategy)
    {
        this.outputDirectory = outputDirectory;
        this.namingStrategy = namingStrategy;
    }

    /// <inheritdoc/>
    public string GenerateTargetInsFilePath(string insFile, ISimulation simulation)
    {
        string simulationName = namingStrategy.GenerateName(simulation);
        string insName = Path.GetFileNameWithoutExtension(insFile);

        // If there are multiple instruction files, include the name of the
        // instruction file in the job name.
        string jobName = GenerateJobName(simulationName, insName);

        // Job directory needs to be deep enough to disambiguate it from other
        // jobs.
        string insSanitised = SanitizePathSegment(insName, true);
        string simulationSanitised = SanitizePathSegment(simulationName, true);
        string jobDirectory = GetJobDirectory(outputDirectory, insSanitised, simulationSanitised);

        string fileName = $"{SanitizePathSegment(jobName, false)}.ins";
        string targetInsFile = Path.Combine(jobDirectory, fileName);
        return targetInsFile;
    }

    /// <inheritdoc/>
    public string GetAbsolutePath(string relative)
    {
        // This will yield the original path if it's rooted. Otherwise, the path
        // will be assumed to be relative to output directory, and an absolute
        // path will be generated.
        return Path.GetFullPath(relative, outputDirectory);
    }

    /// <inheritdoc/>
    public string GetRelativePath(string path)
    {
        string absolute = GetAbsolutePath(path);

        // Return the path of absolute relative to outputDirectory.
        return Path.GetRelativePath(outputDirectory, absolute);
    }

    /// <summary>
    /// Generate a job name for a simulation.
    /// </summary>
    /// <param name="simulationName">The simulation name.</param>
    /// <param name="insName">The instruction file name.</param>
    /// <returns>The job name.</returns>
    protected abstract string GenerateJobName(string simulationName, string insName);

    /// <summary>
    /// Get the job directory for a simulation.
    /// </summary>
    /// <param name="outputDirectory">The base output directory.</param>
    /// <param name="insName">The instruction file name.</param>
    /// <param name="simulationName">The simulation name.</param>
    /// <returns>The job directory.</returns>
    protected abstract string GetJobDirectory(string outputDirectory, string insName, string simulationName);

    /// <summary>
    /// Make a path segment safe for filesystem usage: trim, replace whitespace
    /// with '_', remove invalid chars, collapse repeats, and cap length.
    /// Ensures a non-empty result by falling back to "sim".
    /// </summary>
    protected static string SanitizePathSegment(string input, bool directory, int maxLength = 64)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or whitespace");

        // Replace whitespace with underscore
        StringBuilder sb = new(input.Length);
        foreach (char ch in input.Trim())
        {
            if (char.IsWhiteSpace(ch))
                sb.Append('_');
            else if (!IsInvalidPathChar(ch, directory))
                sb.Append(ch);
        }

        // Enforce max length per segment
        string result = sb.ToString();
        if (result.Length > maxLength)
            result = result[..maxLength];

        return result;
    }

	/// <summary>
	/// Check if a character is invalid for a path segment.
	/// </summary>
	/// <param name="ch">The character to check.</param>
	/// <param name="directory">True if the path is a directory, false otherwise.</param>
	/// <returns>True if the character is invalid, false otherwise.</returns>
	private static bool IsInvalidPathChar(char ch, bool directory)
	{
		// Invalid filename characters plus path separators.
		char[] invalid = directory ? Path.GetInvalidPathChars() : Path.GetInvalidFileNameChars();
		return invalid.Contains(ch)
			   || ch == Path.DirectorySeparatorChar
			   || ch == Path.AltDirectorySeparatorChar;
	}

    private static string CollapseRuns(string input, char[] targets)
    {
        if (string.IsNullOrEmpty(input) || targets.Length == 0)
            return input;

        var set = new HashSet<char>(targets);
        var sb = new StringBuilder(input.Length);
        char? last = null;
        foreach (char ch in input)
        {
            if (last.HasValue && set.Contains(ch) && ch == last.Value)
            {
                continue; // skip duplicate run char
            }
            sb.Append(ch);
            last = ch;
        }
        return sb.ToString();
    }
}
