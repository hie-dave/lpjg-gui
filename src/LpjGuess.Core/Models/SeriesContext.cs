using System.Diagnostics.CodeAnalysis;

namespace LpjGuess.Core.Models;

/// <summary>
/// A coordinate.
/// </summary>
public readonly struct SeriesContext
{
    /// <summary>
    /// The name of the experiment which generated the simulation which produced
    /// the data in this series.
    /// </summary>
    public string ExperimentName { get; private init; }

    /// <summary>
    /// The name of the simulation which produced the data in this series.
    /// </summary>
    public string SimulationName { get; private init; }

    /// <summary>
    /// The latitude of the coordinate.
    /// </summary>
    public Gridcell Gridcell { get; private init; }

    /// <summary>
    /// The layer of the coordinate.
    /// </summary>
    public string Layer { get; private init; }

    /// <summary>
    /// The stand of the coordinate.
    /// </summary>
    public int? Stand { get; private init; }

    /// <summary>
    /// The patch of the coordinate.
    /// </summary>
    public int? Patch { get; private init; }

    /// <summary>
    /// The individual of the coordinate.
    /// </summary>
    public int? Individual { get; private init; }

    /// <summary>
    /// The PFT of the coordinate.
    /// </summary>
    public string? Pft { get; private init; }

    /// <summary>
    /// Create a new <see cref="SeriesContext"/> instance.
    /// </summary>
    /// <param name="experimentName">The name of the experiment.</param>
    /// <param name="simulationName">The name of the simulation.</param>
    /// <param name="gridcell">The gridcell.</param>
    /// <param name="layer">The layer.</param>
    /// <param name="stand">The stand.</param>
    /// <param name="patch">The patch.</param>
    /// <param name="individual">The individual.</param>
    /// <param name="pft">The PFT.</param>
    public SeriesContext(
        string experimentName,
        string simulationName,
        Gridcell gridcell,
        string layer,
        int? stand = null,
        int? patch = null,
        int? individual = null,
        string? pft = null)
    {
        ExperimentName = experimentName;
        SimulationName = simulationName;
        Gridcell = gridcell;
        Layer = layer;
        Stand = stand;
        Patch = patch;
        Individual = individual;
        Pft = pft;
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not SeriesContext other)
            return false;

        return ExperimentName == other.ExperimentName &&
               SimulationName == other.SimulationName &&
               Gridcell.Equals(other.Gridcell) &&
               Layer == other.Layer &&
               Stand == other.Stand &&
               Patch == other.Patch &&
               Individual == other.Individual &&
               Pft == other.Pft;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(ExperimentName, SimulationName, Gridcell, Layer, Stand, Patch, Individual, Pft);
    }
}
