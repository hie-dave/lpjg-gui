namespace LpjGuess.Frontend.Interfaces.Commands;

/// <summary>
/// An interface to a command.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Execute the command.
    /// </summary>
    void Execute();

    /// <summary>
    /// Undo the command.
    /// </summary>
    void Undo();

    /// <summary>
    /// Get a description of the command.
    /// </summary>
    /// <returns>The description of the command.</returns>
    string GetDescription();
}
