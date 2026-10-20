using LpjGuess.Core.Models;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace LpjGuess.Runner;

/// <summary>
/// Public API for running experiments.
/// </summary>
public sealed class ExperimentRunner
{
    /// <summary>
    /// Asynchronously runs an experiment.
    /// </summary>
    /// <param name="config">Configuration for the experiment.</param>
    /// <param name="resolver">Optional path resolver. If null, a static resolver is used.</param>
    /// <param name="reporter">Optional progress reporter. If null, progress is ignored.</param>
    /// <param name="helper">Optional output helper. If null, output is ignored.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="ExperimentResult"/> with summary information.</returns>
    public async Task<ExperimentResult> RunAsync(
        RunnerConfiguration config,
        IPathResolver? resolver,
        IProgressReporter? reporter,
        IOutputHelper? helper,
        CancellationToken ct)
    {
        // TODO: make simulation naming strategy configurable.
        SimulationGeneratorConfig generatorConfig = new SimulationGeneratorConfig(
            config.Settings.Parallel,
            config.Settings.CpuCount,
            config.Factors,
            config.InsFiles,
            config.Pfts,
            new ManualNamingStrategy(),
            new ResultCatalog());
        // Use provided reporter/output, or sensible no-op defaults for library
        // use.
        resolver ??= new StaticPathResolver(config.Settings.OutputDirectory,
                                            generatorConfig.NamingStrategy);
        reporter ??= new NullProgressReporter();
        helper ??= new OutputIgnorer();

        JobManagerConfiguration jobSettings = config.Settings.ToJobManagerConfig();

        SimulationBatch batch = new SimulationBatch(
            resolver,
            generatorConfig,
            config.Settings.InputModule,
            config.Settings.CleanPolicy);
        RunPlan plan = new RunPlan([batch], jobSettings);

        // Library calls must not write unsolicited messages to stdout: callers
        // such as the machine-readable CLI use stdout as a protocol channel.
        ExistingOutputService cleanupService = new ExistingOutputService(
            NullLogger<ExistingOutputService>.Instance);
        RunOrchestrator orchestrator = new RunOrchestrator(cleanupService);
        return await orchestrator.RunAsync(plan, reporter, helper, ct);
    }

    /// <summary>
    /// Asynchronously runs an experiment with default reporter/output.
    /// </summary>
    /// <param name="config">Configuration for the experiment.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="ExperimentResult"/> with summary information.</returns>
    public Task<ExperimentResult> RunAsync(RunnerConfiguration config,
                                           CancellationToken ct)
        => RunAsync(config, resolver: null, reporter: null, helper: null, ct);

    /// <summary>
    /// Runs an experiment synchronously.
    /// </summary>
    /// <param name="config">Configuration for the experiment.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="output">Optional output helper.</param>
    /// <returns>An <see cref="ExperimentResult"/> summarizing the run.</returns>
    public ExperimentResult Run(
        RunnerConfiguration config,
        IProgressReporter? progress = null,
        IOutputHelper? output = null)
    {
        using CancellationTokenSource cts = new CancellationTokenSource();
        return RunAsync(config, resolver: null, progress, output,
                        cts.Token).GetAwaiter().GetResult();
    }
}
