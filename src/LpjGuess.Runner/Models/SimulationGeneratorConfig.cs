using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Services;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Holds configuration settings for the runner tool.
/// </summary>
public class SimulationGeneratorConfig
{
    /// <summary>
    /// Whether to generate jobs in parallel.
    /// </summary>
    public bool Parallel { get; private init; }

    /// <summary>
    /// Number of CPUs to use for job generation if parallel generation is enabled.
    /// </summary>
    public ushort CpuCount { get; private init; }

    /// <summary>
    /// Simulations to generate from each instruction file.
    /// </summary>
    public IReadOnlyList<ISimulation> Simulations { get; private init; }

    /// <summary>
    /// Instruction files.
    /// </summary>
    public IReadOnlyList<string> InsFiles { get; private init; }

    /// <summary>
    /// PFTs to enable.
    /// </summary>
    public IReadOnlyList<string> Pfts { get; private init; }

    /// <summary>
    /// The naming strategy to use.
    /// </summary>
    public ISimulationNamingStrategy NamingStrategy { get; private init; }

    /// <summary>
    /// Simulation manifest reader/writer.
    /// </summary>
    public IResultCatalog Catalog { get; private init; }

    /// <summary>
    /// Create a new runner configuration.
    /// </summary>
    /// <param name="parallel">Whether to generate jobs in parallel.</param>
    /// <param name="cpuCount">Number of CPUs to use for job generation if parallel generation is enabled.</param>
    /// <param name="simulations">Simulations to generate from each instruction file.</param>
    /// <param name="insFiles">Instruction files.</param>
    /// <param name="pfts">The PFTs to enable.</param>
    /// <param name="namingStrategy">The naming strategy to use.</param>
    /// <param name="catalog">The result catalog to use.</param>
    public SimulationGeneratorConfig(
        bool parallel,
        ushort cpuCount,
        IEnumerable<ISimulation> simulations,
        IEnumerable<string> insFiles,
        IEnumerable<string> pfts,
        ISimulationNamingStrategy namingStrategy,
        IResultCatalog catalog)
    {
        Parallel = parallel;
        CpuCount = cpuCount;
        Simulations = simulations.ToList();
        InsFiles = insFiles.ToList();
        Pfts = pfts.ToList();
        NamingStrategy = namingStrategy;
        Catalog = catalog;
    }
}
