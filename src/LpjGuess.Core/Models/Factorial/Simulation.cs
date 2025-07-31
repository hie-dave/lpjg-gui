using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Parsers;

namespace LpjGuess.Core.Models.Factorial;

/// <summary>
/// Represents a simulation, as a set of changes to be applied to a base
/// instruction file.
/// </summary>
public class Simulation : ISimulation
{
    /// <inheritdoc />
    public string Name { get; private init; }

    /// <summary>
    /// List of parameters to be changed in this simulation.
    /// </summary>
    public IEnumerable<IFactor> Changes { get; private init; }

    /// <summary>
    /// Create a new <see cref="Simulation"/> instance.
    /// </summary>
    /// <param name="name">The name of the factors.</param>
    /// <param name="factors">The factors.</param>
    public Simulation(string name, IEnumerable<IFactor> factors)
    {
        Name = name;
        Changes = factors;
    }

    /// <summary>
    /// Create a new <see cref="Simulation"/> instance.
    /// </summary>
    /// <param name="factors">The factors.</param>
    public Simulation(IEnumerable<IFactor> factors)
        : this(factors.Select(f => f.GetName()).Aggregate((x, y) => $"{x}_{y}"), factors) { }

    /// <inheritdoc />
    public void Generate(string insFile, string targetFile, IEnumerable<string> pfts)
    {
        InstructionFileParser ins = InstructionFileParser.FromFile(insFile);

        foreach (IFactor factor in Changes)
            factor.Apply(ins);

        // Disable all PFTs except those required.
        if (pfts.Any())
        {
            ins.DisableAllPfts();
            foreach (string pft in pfts)
                ins.EnablePft(pft);
        }

        // TODO: support async operations.
        string content = ins.GenerateContent();
        File.WriteAllText(targetFile, content);
    }
}
