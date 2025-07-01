using LpjGuess.Core.Models.Graphing;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which controls a graph view.
/// </summary>
public interface IGraphsPresenter : IPresenter<IGraphsView, IReadOnlyList<Graph>>
{
	/// <summary>
	/// Get the graphs as they are currently configured.
	/// </summary>
	IReadOnlyList<Graph> GetGraphs();

	/// <summary>
	/// Refresh all graphs.
	/// </summary>
	void RefreshAll();
}
