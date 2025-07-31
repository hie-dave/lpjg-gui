using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Holds configuration settings for the runner tool.
/// </summary>
public class RunnerConfiguration
{
    /// <summary>
    /// Run settings.
    /// </summary>
    public RunSettings Settings { get; private init; }

    /// <summary>
    /// Factorial combinations.
    /// </summary>
    public IReadOnlyList<IFactors> Factors { get; private init; }

    /// <summary>
    /// Input files.
    /// </summary>
    public IReadOnlyList<string> InsFiles { get; private init; }

    /// <summary>
    /// PFTs.
    /// </summary>
    public IReadOnlyList<string> Pfts { get; private init; }

    /// <summary>
    /// Create a new runner configuration.
    /// </summary>
    /// <param name="settings">Run settings.</param>
    /// <param name="factors">Factorial combinations.</param>
    /// <param name="insFiles">Input files.</param>
    public RunnerConfiguration(
        RunSettings settings,
        IEnumerable<IFactors> factors,
        IEnumerable<string> insFiles,
        IEnumerable<string> pfts)
    {
        Settings = settings;
        Factors = factors.ToList();
        InsFiles = insFiles.ToList();
        Pfts = pfts.ToList();
    }
}
