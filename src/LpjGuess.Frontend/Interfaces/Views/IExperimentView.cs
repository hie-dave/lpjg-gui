using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Runner.Models;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to an experiment view.
/// </summary>
public interface IExperimentView : IView
{
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
        IEnumerable<string> pfts,
        IEnumerable<ParameterGroup> factorials);

    /// <summary>
    /// Update the instruction files.
    /// </summary>
    void UpdateInstructionFiles(IEnumerable<string> instructionFiles);
}
