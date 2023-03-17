using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view displayed in a dialog.
/// </summary>
public interface IDialogView : IView
{
	/// <summary>
	/// Show the dialog.
	/// </summary>
	void Show();

	/// <summary>
	/// Called when the dialog is closed by the user.
	/// </summary>
	Event OnClose { get; }
}
