using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which controls a graph view.
/// </summary>
public interface IGraphsPresenter : IPresenter<IGraphsView>
{
	/// <summary>
	/// Get the graphs as they are currently configured.
	/// </summary>
	IEnumerable<Graph> GetGraphs();

	/// <summary>
	/// Update the instruction files for which data should be displayed.
	/// </summary>
	void UpdateInstructionFiles(IEnumerable<string> instructionFiles);

	/// <summary>
	/// Refresh all graphs.
	/// </summary>
	void RefreshAll();
}
