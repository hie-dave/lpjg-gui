namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface for a presenter which connects to a dialog view.
/// </summary>
public interface IDialogPresenter : IDisposable
{
	/// <summary>
	/// Show the dialog.
	/// </summary>
	void Show();
}
