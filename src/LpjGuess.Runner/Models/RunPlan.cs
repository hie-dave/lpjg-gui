namespace LpjGuess.Runner.Models;

/// <summary>
/// Encapsulates a plan for running a set of simulations.
/// </summary>
/// <param name="Batches">Configuration for the simulations to be run.</param>
/// <param name="JobManagerConfig">Configuration for the job manager.</param>
public sealed record RunPlan(
    IReadOnlyList<SimulationBatch> Batches,
    JobManagerConfiguration JobManagerConfig
);
