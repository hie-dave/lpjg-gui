namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which manages the logs of a simulation.
/// </summary>
public interface ILogsPresenter : IPresenter
{
    /// <summary>
    /// Append a line of text to the view.
    /// </summary>
    /// <param name="text">The text.</param>
    void AppendLine(string text);

    /// <summary>
    /// Clear the view.
    /// </summary>
    void Clear();
}
