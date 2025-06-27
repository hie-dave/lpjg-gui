namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface for a presenter.
/// </summary>
public interface IPresenter : IDisposable
{
    /// <summary>
    /// Get the view owned by this presenter.
    /// </summary>
    /// <returns>The view owned by this presenter.</returns>
    IView GetView();
}

/// <summary>
/// An interface for a presenter.
/// </summary>
public interface IPresenter<out TView, TModel> : IPresenter where TView : IView
{
	/// <summary>
	/// Get the view owned by this presenter.
	/// </summary>
	public new TView GetView();
}
