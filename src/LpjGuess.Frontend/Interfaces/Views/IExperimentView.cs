using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to an experiment view.
/// </summary>
public interface IExperimentView : IView
{
    /// <summary>
    /// The view which displays the factorial data.
    /// </summary>
    IFactorialView FactorialView { get; }

    /// <summary>
    /// Called when the user wants to change the experiment.
    /// </summary>
    Event<IModelChange<Experiment>> OnChanged { get; }

    /// <summary>
    /// Populate the view with the experiment.
    /// </summary>
    void Populate(
        string name,
        string description,
        string runner,
        IEnumerable<string> instructionFiles,
        IEnumerable<string> pfts);

    /// <summary>
    /// Update the instruction files.
    /// </summary>
    void UpdateInstructionFiles(IEnumerable<string> instructionFiles);
}
