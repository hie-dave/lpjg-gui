namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// Interface to the main presenter.
/// </summary>
public interface IMainPresenter : IPresenter<IMainView>
{
    /// <summary>
    /// Initialise the presenter.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitialiseAsync(CancellationToken ct = default);

    /// <summary>
    /// Show the main window.
    /// </summary>
    void Show();
}
