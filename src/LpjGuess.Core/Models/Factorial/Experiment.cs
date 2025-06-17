using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Core.Models.Factorial;

/// <summary>
/// A factorial experiment.
/// </summary>
public class Experiment
{
    /// <summary>
    /// Name of this experiment.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of this experiment.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Name of the runner to use for this experiment.
    /// </summary>
    public string Runner { get; set; }

    /// <summary>
    /// List of instruction files to run.
    /// </summary>
    public List<string> InstructionFiles { get; set; }

    /// <summary>
    /// List of PFTs to enable for this experiment. All others will be disabled.
    /// </summary>
    public List<string> Pfts { get; set; }

    /// <summary>
    /// The simulation generator to use for this experiment.
    /// </summary>
    public ISimulationGenerator SimulationGenerator { get; set; }

    /// <summary>
    /// Create a new <see cref="Experiment"/> instance.
    /// </summary>
    /// <param name="name">Name of this experiment.</param>
    /// <param name="description">Description of this experiment.</param>
    /// <param name="runner">Name of the runner to use for this experiment.</param>
    /// <param name="instructionFiles">List of instruction files to run.</param>
    /// <param name="pfts">List of PFTs to enable for this experiment. All others will be disabled.</param>
    /// <param name="simulationGenerator">List of factorials in this experiment.</param>
    public Experiment(
        string name,
        string description,
        string runner,
        List<string> instructionFiles,
        List<string> pfts,
        ISimulationGenerator simulationGenerator)
    {
        Name = name;
        Description = description;
        Runner = runner;
        InstructionFiles = instructionFiles;
        Pfts = pfts;
        SimulationGenerator = simulationGenerator;
    }
}
