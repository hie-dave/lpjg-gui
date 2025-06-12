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
    /// Create a new <see cref="LocalRunnerConfiguration"/> instance.
    /// </summary>
    /// <param name="guessPath">Path to the lpj-guess executable.</param>
    /// <param name="name">Name of the runner.</param>
    public LocalRunnerConfiguration(string guessPath, string name)
    {
        GuessPath = guessPath;
        Name = name;
    }

    /// <summary>
    /// Constructor provided for serialisation only. Don't call this.
    /// </summary>
    public LocalRunnerConfiguration()
    {
        GuessPath = "";
        Name = "";
    }

    public IRunner CreateRunner(string inputModule)
    {
        return new LocalRunner(this, inputModule);
    }
}
