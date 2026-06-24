using LpjGuess.Core.Models;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Services;
using Microsoft.Extensions.Logging;

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
    /// <param name="cleanupPolicy">Policy for handling existing output.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="ExperimentResult"/> with summary information.</returns>
    public async Task<ExperimentResult> RunAsync(
        RunnerConfiguration config,
        IPathResolver? resolver,
        IProgressReporter? reporter,
        IOutputHelper? helper,
        ExistingOutputPolicy cleanupPolicy,
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
            cleanupPolicy);
        RunPlan plan = new RunPlan([batch], jobSettings);

        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ExistingOutputService>();
        ExistingOutputService cleanupService = new ExistingOutputService(logger);
        RunOrchestrator orchestrator = new RunOrchestrator(cleanupService);
        return await orchestrator.RunAsync(plan, reporter, helper, ct);
    }

    /// <summary>
    /// Asynchronously runs an experiment with default reporter/output.
    /// </summary>
    /// <param name="config">Configuration for the experiment.</param>
    /// <param name="cleanupPolicy">Policy for handling existing output.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="ExperimentResult"/> with summary information.</returns>
    public Task<ExperimentResult> RunAsync(RunnerConfiguration config,
                                           ExistingOutputPolicy cleanupPolicy,
                                           CancellationToken ct)
        => RunAsync(config, resolver: null, reporter: null, helper: null,
                    cleanupPolicy: cleanupPolicy, ct);

    /// <summary>
    /// Runs an experiment synchronously.
    /// </summary>
    /// <param name="config">Configuration for the experiment.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="output">Optional output helper.</param>
    /// <param name="cleanupPolicy">Policy for handling existing output.</param>
    /// <returns>An <see cref="ExperimentResult"/> summarizing the run.</returns>
    public ExperimentResult Run(
        RunnerConfiguration config,
        IProgressReporter? progress = null,
        IOutputHelper? output = null,
        ExistingOutputPolicy cleanupPolicy = ExistingOutputPolicy.Preserve)
    {
        using CancellationTokenSource cts = new CancellationTokenSource();
        return RunAsync(config, resolver: null, progress, output, cleanupPolicy,
                        cts.Token).GetAwaiter().GetResult();
    }
}
