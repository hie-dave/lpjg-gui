using LpjGuess.Core.Extensions;
using LpjGuess.Core.Models;
using LpjGuess.Runner.Models;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Service for handling existing outputs according to specified policies.
/// </summary>
public class ExistingOutputService
{
    private readonly ILogger<ExistingOutputService> logger;

    /// <summary>
    /// Create a new <see cref="ExistingOutputService"/> instance.
    /// </summary>
    /// <param name="logger">The logger to use for logging messages.</param>
    public ExistingOutputService(ILogger<ExistingOutputService> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Apply the specified existing output policy to the given simulation batch.
    /// </summary>
    /// <param name="batch">A plan for running a batch of simulations.</param>
    /// <param name="policy">The policy to apply.</param>
    public void Apply(SimulationBatch batch, ExistingOutputPolicy policy)
    {
        if (policy == ExistingOutputPolicy.Preserve)
            return;

        // Read existing index file.
        IPathResolver resolver = batch.PathResolver;
        IResultCatalog catalog = batch.GeneratorConfig.Catalog;

        HashSet<string> planned = GetPlannedDirectories(batch);
        HashSet<string> indexed = GetIndexedDirectories(resolver, catalog);

        if ((policy & ExistingOutputPolicy.Fail) == ExistingOutputPolicy.Fail)
        {
            List<string> existing = planned.Where(Directory.Exists)
                .Concat(indexed.Where(Directory.Exists))
                .Distinct()
                .ToList();

            if (existing.Count > 0)
            {
                string message = $"Existing output directories found: {string.Join(", ", existing)}";
                throw new InvalidOperationException(message);
            }
        }

        HashSet<string> toDelete = new();
        if ((policy & ExistingOutputPolicy.PruneStale) == ExistingOutputPolicy.PruneStale)
            toDelete.AddRange(indexed.Except(planned));
        if ((policy & ExistingOutputPolicy.CleanManaged) == ExistingOutputPolicy.CleanManaged)
            toDelete.AddRange(planned.Intersect(indexed));

        string outputRoot = Path.GetFullPath(resolver.GetAbsolutePath(""));
        toDelete = toDelete.Where(Directory.Exists)
                           .Where(d => IsUnderDirectory(d, outputRoot))
                           .ToHashSet();

        foreach (string directory in toDelete)
        {
            logger.LogDebug("Deleting existing output directory: {Directory}", directory);
            Directory.Delete(directory, true);
        }
    }

    private static HashSet<string> GetPlannedDirectories(SimulationBatch batch)
    {
        HashSet<string> result = new();

        foreach (string insFile in batch.GeneratorConfig.InsFiles)
        {
            foreach (var simulation in batch.GeneratorConfig.Simulations)
            {
                string insPath = batch.PathResolver
                    .GenerateTargetInsFilePath(insFile, simulation);
                string? directory = Path.GetDirectoryName(insPath);
                if (directory is not null)
                    result.Add(Path.GetFullPath(directory));
            }
        }
        return result;
    }

    private static HashSet<string> GetIndexedDirectories(IPathResolver resolver,
                                                         IResultCatalog catalog)
    {
        try
        {
            return catalog.ReadIndex(resolver)
                          .Simulations
                          .Select(resolver.GetAbsolutePath)
                          .Select(Path.GetFullPath)
                          .ToHashSet();
        }
        catch (FileNotFoundException)
        {
            return [];
        }
    }

    private static bool IsUnderDirectory(string path, string root)
    {
        string fullPath = Path.GetFullPath(path);
        string fullRoot = Path.GetFullPath(root)
                              .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                          + Path.DirectorySeparatorChar;
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return fullPath.StartsWith(fullRoot, comparison);
    }
}
