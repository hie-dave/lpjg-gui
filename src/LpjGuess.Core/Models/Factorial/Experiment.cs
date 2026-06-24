using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Core.Models.Factorial.Generators.Factors;

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
    /// Input module to be used by LPJ-Guess for this experiment.
    /// </summary>
    public string InputModule { get; set; }

    /// <summary>
    /// Policy used when generated output already exists for this experiment.
    /// </summary>
    public ExistingOutputPolicy ExistingOutputPolicy { get; set; }

    /// <summary>
    /// List of instruction files not to run.
    /// </summary>
    public List<string> DisabledInsFiles { get; set; }

    /// <summary>
    /// List of PFTs to enable for this experiment. All others will be disabled.
    /// </summary>
    public List<string> Pfts { get; set; }

    /// <summary>
    /// The simulation generator to use for this experiment.
    /// </summary>
    public ISimulationGenerator SimulationGenerator { get; set; }

    /// <summary>
    /// Create an empty experiment. Intended for deserialization.
    /// </summary>
    public Experiment()
    {
        Name = string.Empty;
        Description = string.Empty;
        Runner = string.Empty;
        InputModule = "nc";
        ExistingOutputPolicy = ExistingOutputPolicy.CleanManaged;
        DisabledInsFiles = [];
        Pfts = [];
        SimulationGenerator = new FactorialGenerator(false, []);
    }

    /// <summary>
    /// Create a new <see cref="Experiment"/> instance.
    /// </summary>
    /// <param name="name">Name of this experiment.</param>
    /// <param name="description">Description of this experiment.</param>
    /// <param name="runner">Name of the runner to use for this experiment.</param>
    /// <param name="inputModule">Input module to use for this experiment.</param>
    /// <param name="existingOutputPolicy">Policy used when generated output already exists.</param>
    /// <param name="disabledInstructionFiles">List of instruction files not to run.</param>
    /// <param name="pfts">List of PFTs to enable for this experiment. All others will be disabled.</param>
    /// <param name="simulationGenerator">List of factorials in this experiment.</param>
    public Experiment(
        string name,
        string description,
        string runner,
        string inputModule,
        ExistingOutputPolicy existingOutputPolicy,
        List<string> disabledInstructionFiles,
        List<string> pfts,
        ISimulationGenerator simulationGenerator)
    {
        Name = name;
        Description = description;
        Runner = runner;
        InputModule = string.IsNullOrWhiteSpace(inputModule) ? "nc" : inputModule;
        ExistingOutputPolicy = existingOutputPolicy;
        DisabledInsFiles = disabledInstructionFiles;
        Pfts = pfts;
        SimulationGenerator = simulationGenerator;
    }

    /// <summary>
    /// Create an experiment that represents a baseline run of all instruction
    /// files in the workspace.
    /// </summary>
    /// <returns>An experiment that represents a baseline run.</returns>
    public static Experiment CreateBaseline()
    {
        SimpleFactorGenerator generator = new("Baseline", [new DummyFactor()]);
        return new Experiment(
            "Baseline",
            "Baseline experiment",
            "baseline",
            "nc",
            ExistingOutputPolicy.CleanManaged,
            new List<string>(),
            new List<string>(),
            new FactorialGenerator(false, [generator]));
    }
}
