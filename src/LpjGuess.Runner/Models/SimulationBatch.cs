using LpjGuess.Core.Models;
using LpjGuess.Runner.Services;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Encapsulates the configuration for a batch of related simulations.
/// </summary>
/// <param name="PathResolver">Resolves paths for simulations.</param>
/// <param name="GeneratorConfig">Configuration for the simulation generator.</param>
/// <param name="InputModule">Input module to use for jobs in this batch.</param>
/// <param name="ExistingOutputPolicy">Policy for handling existing output in this batch.</param>
public sealed record SimulationBatch(
    IPathResolver PathResolver,
    SimulationGeneratorConfig GeneratorConfig,
    string InputModule = "nc",
    ExistingOutputPolicy ExistingOutputPolicy = ExistingOutputPolicy.CleanManaged
);
