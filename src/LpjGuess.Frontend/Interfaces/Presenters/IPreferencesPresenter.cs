using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// Interface to the preferences presenter.
/// </summary>
public interface IPreferencesPresenter : IDialogPresenter
{
    /// <summary>
    /// Called when the dialog is closed by the user.
    /// </summary>
    Event OnClosed { get; }
}
