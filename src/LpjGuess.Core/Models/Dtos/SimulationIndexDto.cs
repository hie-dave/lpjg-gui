namespace LpjGuess.Core.Models.Dtos;

/// <summary>
/// DTO for serialisation of <see cref="SimulationIndex"/>.
/// </summary>
public class SimulationIndexDto
{
    /// <summary>
    /// The list of simulations.
    /// </summary>
    public List<string> Simulations { get; set; }

    /// <summary>
    /// Creates a default <see cref="SimulationIndexDto"/>.
    /// </summary>
    public SimulationIndexDto()
    {
        Simulations = [];
    }

    /// <summary>
    /// Creates a <see cref="SimulationIndexDto"/> from a
    /// <see cref="SimulationIndex"/>.
    /// </summary>
    public static SimulationIndexDto FromSimulationIndex(SimulationIndex index)
    {
        return new SimulationIndexDto
        {
            Simulations = index.Simulations.ToList()
        };
    }

    /// <summary>
    /// Creates a <see cref="SimulationIndex"/> from a
    /// <see cref="SimulationIndexDto"/>.
    /// </summary>
    public SimulationIndex ToSimulationIndex()
    {
        return new SimulationIndex(Simulations);
    }
}
