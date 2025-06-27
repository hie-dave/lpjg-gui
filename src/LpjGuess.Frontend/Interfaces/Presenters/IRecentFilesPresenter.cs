using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// Interface for a recent files presenter.
/// </summary>
public interface IRecentFilesPresenter : IPresenter<IRecentFilesView, Configuration>
{
	/// <summary>
	/// Event raised when a recent file is selected.
	/// </summary>
	public Event<string> OnOpenFile { get; }
}
