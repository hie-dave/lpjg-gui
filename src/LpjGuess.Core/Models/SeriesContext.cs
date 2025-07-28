using System.Diagnostics.CodeAnalysis;
using LpjGuess.Core.Models.Importer;

namespace LpjGuess.Core.Models;

/// <summary>
/// A coordinate.
/// </summary>
public struct SeriesContext
{
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
    /// The name of the simulation.
    /// NOTE: only valid for model output series. Should refactor this class.
    /// </summary>
    public string? SimulationName { get; private init; }

    /// <summary>
    /// The PFT of the coordinate.
    /// </summary>
    public string? Pft { get; private init; }

    /// <summary>
    /// Create a new <see cref="SeriesContext"/> instance.
    /// </summary>
    /// <param name="gridcell">The gridcell.</param>
    /// <param name="layer">The layer.</param>
    /// <param name="stand">The stand.</param>
    /// <param name="patch">The patch.</param>
    /// <param name="individual">The individual.</param>
    /// <param name="simulationName">The name of the simulation.</param>
    /// <param name="pft">The PFT.</param>
    public SeriesContext(
        Gridcell gridcell,
        string layer,
        int? stand = null,
        int? patch = null,
        int? individual = null,
        string? simulationName = null,
        string? pft = null)
    {
        Gridcell = gridcell;
        Layer = layer;
        Stand = stand;
        Patch = patch;
        Individual = individual;
        SimulationName = simulationName;
        Pft = pft;
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not SeriesContext other)
            return false;

        return Gridcell.Equals(other.Gridcell) &&
               Layer == other.Layer &&
               Stand == other.Stand &&
               Patch == other.Patch &&
               Individual == other.Individual &&
               SimulationName == other.SimulationName &&
               Pft == other.Pft;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Gridcell, Layer, Stand, Patch, Individual, SimulationName);
    }
}
