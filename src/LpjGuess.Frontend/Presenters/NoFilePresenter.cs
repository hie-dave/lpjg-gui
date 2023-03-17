using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// Presenter for a <see cref="NoFileView"/>. This view doesn't do anything
/// (it's a placeholder), so this presenter doesn't really do anything either.
/// </summary>
public class NoFilePresenter : PresenterBase<NoFileView>
{
	/// <summary>
	/// Create a new <see cref="NoFilePresenter"/> instance.
	/// </summary>
	public NoFilePresenter() : base(new NoFileView())
	{
	}
}
