namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface for a presenter.
/// </summary>
/// <remarks>
/// Could make this type generic on the view type.
/// </remarks>
public interface IPresenter : IDisposable
{
	/// <summary>
	/// Get the view owned by this presenter.
	/// </summary>
	public IView GetView();
}
