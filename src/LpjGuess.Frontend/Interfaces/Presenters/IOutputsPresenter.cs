using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// Interface to a presenter which controls an outputs view to display the raw
/// outputs from a model run.
/// </summary>
public interface IOutputsPresenter : IPresenter<IOutputsView>
{
    /// <summary>
    /// Populate the outputs view with the outputs from the given instruction
    /// files.
    /// </summary>
    /// <param name="instructionFiles">A list of instruction files.</param>
    void Populate(IEnumerable<string> instructionFiles);
}
