using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Data.Providers;

/// <summary>
/// A class that provides access to experiments in the workspace.
/// </summary>
public class ExperimentProvider : IExperimentProvider
{
    /// <summary>
    /// The experiments in the workspace.
    /// </summary>
    private List<Experiment> experiments;

    /// <inheritdoc/>
    public Event<IEnumerable<Experiment>> OnExperimentsChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="ExperimentProvider"/> instance.
    /// </summary>
    public ExperimentProvider() : this([])
    {
    }

    /// <summary>
    /// Create a new <see cref="ExperimentProvider"/> instance.
    /// </summary>
    /// <param name="experiments">The experiments.</param>
    public ExperimentProvider(IEnumerable<Experiment> experiments)
    {
        this.experiments = experiments.ToList();
        OnExperimentsChanged = new Event<IEnumerable<Experiment>>();
    }

    /// <summary>
    /// Update the experiments.
    /// </summary>
    /// <param name="experiments">The experiments.</param>
    public void UpdateExperiments(IEnumerable<Experiment> experiments)
    {
        this.experiments = experiments.ToList();
        OnExperimentsChanged.Invoke(this.experiments);
    }

    /// <inheritdoc/>
    public IEnumerable<Experiment> GetExperiments() => experiments;
}
