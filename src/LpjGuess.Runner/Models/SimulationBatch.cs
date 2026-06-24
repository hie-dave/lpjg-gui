using LpjGuess.Runner.Services;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Encapsulates the configuration for a batch of related simulations.
/// </summary>
/// <param name="PathResolver">Resolves paths for simulations.</param>
/// <param name="GeneratorConfig">Configuration for the simulation generator.</param>
public sealed record SimulationBatch(
    IPathResolver PathResolver,
    SimulationGeneratorConfig GeneratorConfig
);
