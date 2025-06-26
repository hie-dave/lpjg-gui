namespace LpjGuess.Frontend.Classes;

/// <summary>
/// A high-level overview of a set of changes to an instruction file.
/// </summary>
public class SimulationDescription
{
    /// <summary>
    /// The name of the simulation
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// The changes this simulation makes to the base instruction file
    /// </summary>
    public IReadOnlyList<ParameterChange> Changes { get; private init; }

    /// <summary>
    /// Create a new <see cref="SimulationDescription"/> instance.
    /// </summary>
    /// <param name="name">The name of the simulation.</param>
    /// <param name="changes">The changes this simulation makes to the base instruction file.</param>
    public SimulationDescription(string name, IEnumerable<ParameterChange> changes)
    {
        Name = name;
        Changes = changes.ToList();
    }
}
