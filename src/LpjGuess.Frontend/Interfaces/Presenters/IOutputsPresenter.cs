namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// Interface to a presenter which controls an outputs view to display the raw
/// outputs from a model run.
/// </summary>
public interface IOutputsPresenter : IPresenter
{
    /// <summary>
    /// Refresh the contents of the view with the currently-available output
    /// files.
    /// </summary>
    void Refresh();
}
