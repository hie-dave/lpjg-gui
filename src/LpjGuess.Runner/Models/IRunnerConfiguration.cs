namespace LpjGuess.Runner.Models;

/// <summary>
/// An interface to a class which describes how to run lpj-guess. Data in this
/// class is not particular to running a specific simulation, but rather it
/// provides a way of running lpj-guess.
/// </summary>
public interface IRunnerConfiguration
{
    /// <summary>
    /// Path to the lpj-guess executable.
    /// </summary>
    /// <remarks>
    /// Does this belong in the interface?
    /// </remarks>
    string GuessPath { get; set; }

    /// <summary>
    /// Name of this runner (used for display purposes only).
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Create a <see cref="IRunner"/> instance for this configuration.
    /// </summary>
    /// <param name="inputModule">The input module to use.</param>
    /// <returns>An <see cref="IRunner"/> instance.</returns>
    IRunner CreateRunner(string inputModule);
}
