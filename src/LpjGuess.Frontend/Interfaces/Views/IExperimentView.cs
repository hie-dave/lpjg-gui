using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to an experiment view.
/// </summary>
public interface IExperimentView : IView
{
    /// <summary>
    /// Set the factorial view.
    /// </summary>
    void SetFactorialView(IFactorialView factorialView);

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
    /// <param name="inputModule">The input module to use.</param>
    /// <param name="existingOutputPolicy">Policy used when generated output already exists.</param>
    /// <param name="instructionFiles">The available instruction files, and whether they are selected in this experiment.</param>
    /// <param name="inheritPfts">Whether generated files retain their base PFT enablement.</param>
    /// <param name="pfts">Available PFTs, their base enablement, and explicit selection state.</param>
    void Populate(
        string name,
        string description,
        string runner,
        string inputModule,
        ExistingOutputPolicy existingOutputPolicy,
        IEnumerable<(string, bool)> instructionFiles,
        bool inheritPfts,
        IEnumerable<(string Name, bool EnabledByDefault, bool Selected)> pfts);

    /// <summary>
    /// Update the generated-design summary and bounded simulation preview.
    /// </summary>
    /// <param name="analysis">Counts and validation results.</param>
    /// <param name="simulations">Descriptions of previewed simulations.</param>
    /// <param name="truncated">Whether additional simulations were omitted.</param>
    void PopulatePreview(
        ExperimentDesignAnalysis analysis,
        IEnumerable<SimulationDescription> simulations,
        bool truncated);

    /// <summary>
    /// Update the instruction files.
    /// </summary>
    /// <param name="instructionFiles">The available instruction files, and whether they are selected in this experiment.</param>
    void UpdateInstructionFiles(IEnumerable<(string, bool)> instructionFiles);
}
