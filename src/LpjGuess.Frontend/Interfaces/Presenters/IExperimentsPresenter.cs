using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which controls an experiments view.
/// </summary>
public interface IExperimentsPresenter : IPresenter<IExperimentsView, IEnumerable<Experiment>>
{
    /// <summary>
    /// Get the experiments as they are currently configured.
    /// </summary>
    IEnumerable<Experiment> GetExperiments();

    /// <summary>
    /// Populate the view with the given experiments.
    /// </summary>
    /// <param name="instructionFiles">The instruction files in the workspace.</param>
    void Populate(IEnumerable<string> instructionFiles);

	/// <summary>
	/// Update the instruction files in the workspace.
	/// </summary>
    /// <remarks>
    /// This will typically be called after the user adds or removes instruction
	/// files from the workspace.
    /// </remarks>
	/// <param name="instructionFiles">The instruction files in the workspace.</param>
	void UpdateInstructionFiles(IEnumerable<string> instructionFiles);
}
