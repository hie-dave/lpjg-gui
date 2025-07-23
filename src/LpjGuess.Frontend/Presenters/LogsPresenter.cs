using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a view which displays the logs of a simulation.
/// </summary>
[RegisterStandalonePresenter(typeof(ILogsPresenter))]
public class LogsPresenter : ILogsPresenter
{
    /// <summary>
    /// The view owned by this presenter.
    /// </summary>
    private readonly IEditorView view;

    /// <summary>
    /// Create a new <see cref="LogsPresenter"/> instance.
    /// </summary>
    /// <param name="view">The view object.</param>
    public LogsPresenter(IEditorView view)
    {
        this.view = view;
        view.Editable = false;
    }

    /// <inheritdoc/>
    public void AppendLine(string text) => view.AppendLine(text);

    /// <inheritdoc/>
    public void Clear() => view.Clear();

    /// <inheritdoc/>
    public void Dispose()
    {
        view.Dispose();
    }

    /// <inheritdoc/>
    public IView GetView() => view;
}
