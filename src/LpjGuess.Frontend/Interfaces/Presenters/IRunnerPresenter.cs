using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// A presenter for a class which displays configuration settings for a runner.
/// </summary>
public interface IRunnerPresenter : IView
{
	/// <summary>
	/// Get the view object.
	/// </summary>
	IGroupView CreateView();

	/// <summary>
	/// Get metadata for this runner.
	/// </summary>
	IRunnerMetadata GetMetadata();
}
