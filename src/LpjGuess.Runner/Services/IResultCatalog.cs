using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models;
using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Manages recording and querying simulation result metadata (manifests and index).
/// </summary>
public interface IResultCatalog
{
    /// <summary>
    /// Write a simulation index to the output root.
    /// </summary>
    /// <param name="pathResolver">The path resolver.</param>
    /// <param name="index">The simulation index to write.</param>
    void WriteIndex(IPathResolver pathResolver, SimulationIndex index);

    /// <summary>
    /// Record a generated simulation by writing a per-simulation manifest and updating the run index.
    /// </summary>
    /// <param name="manifest">The simulation manifest to write.</param>
    void WriteSimulation(SimulationManifest manifest);

    /// <summary>
    /// Read a per-simulation manifest from a simulation directory.
    /// </summary>
    /// <param name="simulationDirectory">Path to the directory containing the simulation artifacts.</param>
    /// <returns>A <see cref="SimulationManifest"/>.</returns>
    SimulationManifest ReadManifest(string simulationDirectory);

    /// <summary>
    /// Read the simulation index under the output root.
    /// </summary>
    /// <param name="pathResolver">The path resolver.</param>
    /// <returns>A <see cref="SimulationIndex"/> instance (empty if none exists).</returns>
    SimulationIndex ReadIndex(IPathResolver pathResolver);
}
