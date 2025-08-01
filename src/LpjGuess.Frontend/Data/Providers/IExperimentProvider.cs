using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Data.Providers;

/// <summary>
/// Interface to a class that provides access to experiments in the workspace.
/// </summary>
public interface IExperimentProvider
{
    /// <summary>
    /// Event raised when the experiments change.
    /// </summary>
    Event<IEnumerable<Experiment>> OnExperimentsChanged { get; }

    /// <summary>
    /// Get the experiments in the workspace.
    /// </summary>
    /// <returns>The experiments in the workspace.</returns>
    public IEnumerable<Experiment> GetExperiments();
}
