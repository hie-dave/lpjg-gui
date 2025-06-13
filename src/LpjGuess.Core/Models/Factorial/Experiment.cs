using LpjGuess.Runner.Models;

namespace LpjGuess.Core.Models.Factorial;

/// <summary>
/// A factorial experiment.
/// </summary>
public class Experiment
{
    /// <summary>
    /// Name of this experiment.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of this experiment.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Name of the runner to use for this experiment.
    /// </summary>
    public string Runner { get; set; } = string.Empty;

    /// <summary>
    /// List of instruction files to run.
    /// </summary>
    public List<string> InstructionFiles { get; set; } = new();

    /// <summary>
    /// List of PFTs to enable for this experiment. All others will be disabled.
    /// </summary>
    public List<string> Pfts { get; set; } = new();

    /// <summary>
    /// List of factorials in this experiment.
    /// </summary>
    public List<ParameterGroup> Factorials { get; set; } = new();
}
