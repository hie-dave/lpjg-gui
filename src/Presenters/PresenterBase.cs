using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// Base class for all presenters.
/// </summary>
/// <typeparam name="TView"></typeparam>
public abstract class PresenterBase<TView> : IPresenter
	where TView : IView
{
	/// <summary>
	/// The view object.
	/// </summary>
	protected readonly TView view;

	/// <summary>
	/// Constructor for this presenter.
	/// </summary>
	/// <param name="view"></param>
	/// <param name="model"></param>
	public PresenterBase(TView view)
	{
		this.view = view;
	}

	/// <inheritdoc />
	public IView GetView() => view;

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		view.Dispose();
	}
}
