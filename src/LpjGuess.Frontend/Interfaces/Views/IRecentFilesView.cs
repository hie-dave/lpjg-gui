using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Interface to a view which displays a list of recently files.
/// </summary>
public interface IRecentFilesView : IView
{
    /// <summary>
    /// Populate the view with a list of files.
    /// </summary>
    /// <param name="files">The files to display.</param>
    void Populate(IEnumerable<string> files);

    /// <summary>
    /// Called when the user has clicked on a recent file to open it.
    /// </summary>
    Event<string> OnClick { get; }
}
