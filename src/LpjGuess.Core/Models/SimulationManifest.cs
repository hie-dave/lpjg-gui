using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Core.Models;

/// <summary>
/// Summary information about a single simulation, persisted next to its input
/// file.
/// </summary>
/// <param name="Key">The unique key for the simulation.</param>
/// <param name="Name">The (display) name of the simulation.</param>
/// <param name="Path">The path to the simulation directory.</param>
/// <param name="BaseIns">The path to the base instruction file.</param>
/// <param name="InsFile">The path to the generated instruction file.</param>
/// <param name="Pfts">The list of PFTs enabled in the simulation.</param>
/// <param name="Factors">The list of factors applied to the simulation.</param>
/// <param name="GeneratedAtUtc">The date and time the simulation was generated.</param>
public sealed record SimulationManifest(
    string Key,
    string Name,
    string Path,
    string BaseIns,
    string InsFile,
    IReadOnlyList<string> Pfts,
    IReadOnlyList<IFactor> Factors,
    DateTime GeneratedAtUtc
);
