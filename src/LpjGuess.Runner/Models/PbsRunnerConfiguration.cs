namespace LpjGuess.Runner.Models;

/// <summary>
/// Configuration parameters for a PBS job.
/// </summary>
public class PbsRunnerConfiguration : IRunnerConfiguration
{
    /// <inheritdoc />
    public string GuessPath { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <summary>
    /// True to create job directory tree but not submit job. False to
    /// additionally submit the job to PBS for execution.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// Job output directory.
    /// </summary>
    public string OutputDirectory { get; set; }

    /// <summary>
    /// Number of CPUs to allocate to the job.
    /// </summary>
    public int CpuCount { get; set; }

    /// <summary>
    /// Maximum walltime allowed for the job.
    /// </summary>
    public TimeSpan Walltime { get; set; }

    /// <summary>
    /// Amount of memory to be allocated to the job.
    /// </summary>
    public uint Memory { get; set; }

    /// <summary>
    /// Queue to which the job should be submitted.
    /// </summary>
    public string Queue { get; set; }

    /// <summary>
    /// PBS project under which the job should be submitted.
    /// </summary>
    public string Project { get; set; }

    /// <summary>
    /// True to send email notifications.
    /// </summary>
    public bool EmailNotifications { get; set; }

    /// <summary>
    /// Email address to which notifications should be sent.
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Create a new <see cref="PbsRunnerConfiguration"/> instance.
    /// </summary>
    /// <param name="guessPath"></param>
    /// <param name="name"></param>
    /// <param name="jobName"></param>
    /// <param name="dryRun"></param>
    /// <param name="outputDirectory"></param>
    /// <param name="cpuCount"></param>
    /// <param name="walltime"></param>
    /// <param name="memory"></param>
    /// <param name="queue"></param>
    /// <param name="project"></param>
    /// <param name="emailNotifications"></param>
    /// <param name="emailAddress"></param>
    public PbsRunnerConfiguration(
        string guessPath,
        string name,
        bool dryRun,
        string outputDirectory,
        int cpuCount,
        TimeSpan walltime,
        uint memory,
        string queue,
        string project,
        bool emailNotifications,
        string? emailAddress
    )
    {
        GuessPath = guessPath;
        Name = name;
        DryRun = dryRun;
        OutputDirectory = outputDirectory;
        CpuCount = cpuCount;
        Walltime = walltime;
        Memory = memory;
        Queue = queue;
        Project = project;
        EmailNotifications = emailNotifications;
        EmailAddress = emailAddress;
        if (emailNotifications && string.IsNullOrWhiteSpace(emailAddress))
            throw new InvalidOperationException($"Email address must be provided if email notifications are enabled.");
    }

    /// <summary>
    /// Constructor provided for serialization purposes only. Don't call this.
    /// </summary>
    public PbsRunnerConfiguration()
    {
        GuessPath = string.Empty;
        Name = string.Empty;
        OutputDirectory = string.Empty;
        Queue = string.Empty;
        Project = string.Empty;
        EmailAddress = null;
    }

    /// <inheritdoc />
    public IRunner CreateRunner(string inputModule)
    {
        return new PbsRunner(this, inputModule);
    }
}
