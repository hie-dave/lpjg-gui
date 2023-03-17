using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// Base class for all presenters.
/// </summary>
/// <typeparam name="TView"></typeparam>
public abstract class PresenterBase<TView> : IPresenter<TView>
	where TView : IView
{
	/// <summary>
	/// The view object.
	/// </summary>
	protected readonly TView view;

	/// <summary>
	/// Create a new <see cref="PresenterBase{TView}"/> instance.
	/// </summary>
	/// <param name="view">The view.</param>
	public PresenterBase(TView view)
	{
		this.view = view;
	}

	/// <inheritdoc />
	public TView GetView() => view;

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		view.Dispose();
	}
}
