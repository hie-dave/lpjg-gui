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
    /// <param name="name">The name of the experiment.</param>
    /// <param name="description">The description of the experiment.</param>
    /// <param name="runner">The runner to use.</param>
    /// <param name="instructionFiles">The available instruction files, and whether they are selected in this experiment.</param>
    /// <param name="pfts">The PFTs.</param>
    void Populate(
        string name,
        string description,
        string runner,
        IEnumerable<(string, bool)> instructionFiles,
        IEnumerable<string> pfts);

    /// <summary>
    /// Update the instruction files.
    /// </summary>
    /// <param name="instructionFiles">The available instruction files, and whether they are selected in this experiment.</param>
    void UpdateInstructionFiles(IEnumerable<(string, bool)> instructionFiles);
}
