namespace LpjGuess.Runner.Models;

/// <summary>
/// Configuration settings which describe a way of running lpj-guess on the
/// local machine (with no parallelisation).
/// </summary>
public class LocalRunnerConfiguration : IRunnerConfiguration
{
    /// <inheritdoc />
    public string GuessPath { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <summary>
    /// Whether to use CPU affinity to pin the process to a single CPU.
    /// </summary>
    public bool UseCpuAffinity { get; set; }

    /// <summary>
    /// Environment variables to override when running the model. `null` values remove the variable.
    /// </summary>
    public IReadOnlyDictionary<string, string?> EnvironmentOverrides { get; }

    /// <summary>
    /// Create a new <see cref="LocalRunnerConfiguration"/> instance.
    /// </summary>
    /// <param name="guessPath">Path to the lpj-guess executable.</param>
    /// <param name="name">Name of the runner.</param>
    /// <param name="useCpuAffinity">Whether to use CPU affinity to pin the process to a single CPU.</param>
    /// <param name="environmentOverrides">Environment variables to override when running the model.</param>
    public LocalRunnerConfiguration(
        string guessPath,
        string name,
        bool useCpuAffinity,
        IReadOnlyDictionary<string, string?> environmentOverrides)
    {
        GuessPath = guessPath;
        Name = name;
        UseCpuAffinity = useCpuAffinity;
        EnvironmentOverrides = environmentOverrides;
    }

    /// <summary>
    /// Constructor provided for serialisation only. Don't call this.
    /// </summary>
    public LocalRunnerConfiguration()
    {
        GuessPath = "";
        Name = "";
        UseCpuAffinity = true;
        EnvironmentOverrides = new Dictionary<string, string?>();
    }

    /// <inheritdoc />
    public IRunner CreateRunner(string inputModule)
    {
        return new LocalRunner(this, inputModule);
    }
}
