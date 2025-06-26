namespace LpjGuess.Frontend.Interfaces.Commands;

/// <summary>
/// An interface to a command registry.
/// </summary>
public interface ICommandRegistry
{
    /// <summary>
    /// Whether there is a command to undo.
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// Whether there is a command to redo.
    /// </summary>
    bool CanRedo { get; }

    /// <summary>
    /// Execute a command.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    void Execute(ICommand command);

    /// <summary>
    /// Undo the most recently executed command.
    /// </summary>
    /// <returns>True if a command was undone, false otherwise.</returns>
    bool Undo();

    /// <summary>
    /// Redo the most recently undone command.
    /// </summary>
    /// <returns>True if a command was redone, false otherwise.</returns>
    bool Redo();

    /// <summary>
    /// Clear the command history.
    /// </summary>
    void ClearHistory();
}
