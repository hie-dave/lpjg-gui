using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// A view which displays a list of experiments.
/// </summary>
public interface IExperimentsView : IView
{
    /// <summary>
    /// The text to be displayed on the add button.
    /// </summary>
    string AddText { get; set; }

    /// <summary>
    /// The event to be raised when the user wants to add an experiment.
    /// </summary>
    Event OnAdd { get; }

    /// <summary>
    /// The event to be raised when the user wants to remove an experiment.
    /// </summary>
    Event<Experiment> OnRemove { get; }

    /// <summary>
    /// Populate the view with the given experiments.
    /// </summary>
    /// <param name="experiments">The experiments to populate the view with.</param>
    void Populate(IEnumerable<(Experiment, IExperimentView)> experiments);

    /// <summary>
    /// Rename an experiment.
    /// </summary>
    /// <param name="experiment">The experiment to rename.</param>
    /// <param name="newName">The new name for the experiment.</param>
    void Rename(Experiment experiment, string newName);
}
