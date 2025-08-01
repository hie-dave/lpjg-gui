using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Holds configuration settings for the runner tool.
/// </summary>
public class SimulationGeneratorConfig
{
    /// <summary>
    /// Output directory into which jobs will be generated.
    /// </summary>
    public string OutputDirectory { get; private init; }

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
    /// Create a new runner configuration.
    /// </summary>
    /// <param name="outputDirectory">Output directory into which jobs will be generated.</param>
    /// <param name="parallel">Whether to generate jobs in parallel.</param>
    /// <param name="cpuCount">Number of CPUs to use for job generation if parallel generation is enabled.</param>
    /// <param name="simulations">Simulations to generate from each instruction file.</param>
    /// <param name="insFiles">Instruction files.</param>
    /// <param name="pfts">The PFTs to enable.</param>
    public SimulationGeneratorConfig(
        string outputDirectory,
        bool parallel,
        ushort cpuCount,
        IEnumerable<ISimulation> simulations,
        IEnumerable<string> insFiles,
        IEnumerable<string> pfts)
    {
        OutputDirectory = outputDirectory;
        Parallel = parallel;
        CpuCount = cpuCount;
        Simulations = simulations.ToList();
        InsFiles = insFiles.ToList();
        Pfts = pfts.ToList();
    }
}
