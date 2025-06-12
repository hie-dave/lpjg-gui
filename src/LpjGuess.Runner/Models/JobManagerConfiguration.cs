namespace LpjGuess.Runner.Models;

public class JobManagerConfiguration
{
    /// <summary>
    /// Configuration settings describing how the jobs should be run.
    /// </summary>
    public IRunnerConfiguration RunConfig { get; private init; }

    /// <summary>
    /// Max number of CPUs to use.
    /// </summary>
    public int CpuCount { get; private init; }

    /// <summary>
    /// True to do a dry run. False to actually run jobs.
    /// </summary>
    public bool DryRun { get; private init; }

    /// <summary>
    /// The input module to use for the jobs.
    /// </summary>
    public string InputModule { get; private init; }

    /// <summary>
    /// Create a new <see cref="JobManagerConfiguration"/> instance.
    /// </summary>
    /// <param name="runConfig">Configuration settings describing how the jobs should be run.</param>
    /// <param name="cpuCount">Max number of CPUs to use.</param>
    /// <param name="dryRun">True to do a dry run. False to actually run jobs.</param>
    /// <param name="inputModule">The input module to use for the jobs.</param>
    public JobManagerConfiguration(
        IRunnerConfiguration runConfig,
        int cpuCount,
        bool dryRun,
        string inputModule)
    {
        RunConfig = runConfig;
        CpuCount = cpuCount;
        DryRun = dryRun;
        InputModule = inputModule;
    }
}
