using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// A registry for managing command execution in the application, including undo/redo functionality.
/// </summary>
public class CommandRegistry : ICommandRegistry
{
    /// <summary>
    /// Maximum number of commands to keep in history.
    /// </summary>
    private readonly int maxHistorySize;

    /// <summary>
    /// History of executed commands.
    /// </summary>
    private readonly List<ICommand> commandHistory;

    /// <summary>
    /// Current position in the command history.
    /// </summary>
    private int currentIndex;

    /// <summary>
    /// Get whether undo is available.
    /// </summary>
    public bool CanUndo => currentIndex >= 0;

    /// <summary>
    /// Get whether redo is available.
    /// </summary>
    public bool CanRedo => currentIndex < commandHistory.Count - 1;

    /// <summary>
    /// Get the singleton instance of the command registry.
    /// </summary>
    public static CommandRegistry Instance { get; } = new CommandRegistry();

    /// <summary>
    /// Event fired before a command is executed.
    /// </summary>
    public Event<ICommand> BeforeCommandExecuted { get; }

    /// <summary>
    /// Event fired after a command is executed.
    /// </summary>
    public Event<ICommand> AfterCommandExecuted { get; }

    /// <summary>
    /// Event fired before a command is undone.
    /// </summary>
    public Event<ICommand> BeforeCommandUndone { get; }

    /// <summary>
    /// Event fired after a command is undone.
    /// </summary>
    public Event<ICommand> AfterCommandUndone { get; }

    /// <summary>
    /// Event fired when the undo/redo state changes (e.g., when commands are executed, undone, or redone).
    /// </summary>
    public Event<CommandHistoryState> HistoryStateChanged { get; }

    /// <summary>
    /// Create a new <see cref="CommandRegistry"/> instance.
    /// </summary>
    /// <param name="maxHistorySize">Maximum number of commands to keep in history.</param>
    private CommandRegistry(int maxHistorySize = 100)
    {
        BeforeCommandExecuted = new Event<ICommand>();
        AfterCommandExecuted = new Event<ICommand>();
        BeforeCommandUndone = new Event<ICommand>();
        AfterCommandUndone = new Event<ICommand>();
        HistoryStateChanged = new Event<CommandHistoryState>();

        commandHistory = new List<ICommand>();
        currentIndex = -1;
        this.maxHistorySize = maxHistorySize;
    }

    /// <summary>
    /// Execute a command and register it in the command history.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    public void Execute(ICommand command)
    {
        // If we're not at the end of the history, remove all commands after the
        // current index.
        if (currentIndex < commandHistory.Count - 1)
            commandHistory.RemoveRange(currentIndex + 1, commandHistory.Count - currentIndex - 1);

        BeforeCommandExecuted.Invoke(command);

        try
        {
            command.Execute();

            // Add the command to history.
            commandHistory.Add(command);
            currentIndex++;

            // Trim history if it exceeds the maximum size.
            if (commandHistory.Count > maxHistorySize)
            {
                commandHistory.RemoveAt(0);
                currentIndex--;
            }
        }
        finally
        {
            AfterCommandExecuted.Invoke(command);
            NotifyHistoryStateChanged();
        }
    }

    /// <summary>
    /// Undo the most recently executed command.
    /// </summary>
    /// <returns>True if a command was undone, false otherwise.</returns>
    public bool Undo()
    {
        if (!CanUndo)
            return false;

        ICommand command = commandHistory[currentIndex];

        BeforeCommandUndone.Invoke(command);

        try
        {
            command.Undo();
            currentIndex--;
        }
        finally
        {
            AfterCommandUndone.Invoke(command);
            NotifyHistoryStateChanged();
        }

        return true;
    }

    /// <summary>
    /// Redo the most recently undone command.
    /// </summary>
    /// <returns>True if a command was redone, false otherwise.</returns>
    public bool Redo()
    {
        if (!CanRedo)
            return false;

        ICommand command = commandHistory[currentIndex + 1];

        BeforeCommandExecuted.Invoke(command);

        try
        {
            command.Execute();
            currentIndex++;
        }
        finally
        {
            AfterCommandExecuted.Invoke(command);
            NotifyHistoryStateChanged();
        }

        return true;
    }

    /// <summary>
    /// Clear the command history.
    /// </summary>
    public void ClearHistory()
    {
        commandHistory.Clear();
        currentIndex = -1;
        NotifyHistoryStateChanged();
    }

    /// <summary>
    /// Get the current command history state.
    /// </summary>
    /// <returns>The current command history state.</returns>
    public CommandHistoryState GetHistoryState()
    {
        return new CommandHistoryState(
            CanUndo,
            CanRedo,
            CanUndo ? commandHistory[currentIndex].GetDescription() : string.Empty,
            CanRedo ? commandHistory[currentIndex + 1].GetDescription() : string.Empty
        );
    }

    /// <summary>
    /// Notify listeners that the history state has changed.
    /// </summary>
    private void NotifyHistoryStateChanged()
    {
        HistoryStateChanged.Invoke(GetHistoryState());
    }
}

/// <summary>
/// Represents the state of the command history.
/// </summary>
public class CommandHistoryState
{
    /// <summary>
    /// Whether undo is available.
    /// </summary>
    public bool CanUndo { get; }

    /// <summary>
    /// Whether redo is available.
    /// </summary>
    public bool CanRedo { get; }

    /// <summary>
    /// Description of the command that would be undone.
    /// </summary>
    public string UndoDescription { get; }

    /// <summary>
    /// Description of the command that would be redone.
    /// </summary>
    public string RedoDescription { get; }

    /// <summary>
    /// Create a new <see cref="CommandHistoryState"/> instance.
    /// </summary>
    /// <param name="canUndo">Whether undo is available.</param>
    /// <param name="canRedo">Whether redo is available.</param>
    /// <param name="undoDescription">Description of the command that would be undone.</param>
    /// <param name="redoDescription">Description of the command that would be redone.</param>
    public CommandHistoryState(bool canUndo, bool canRedo, string undoDescription, string redoDescription)
    {
        CanUndo = canUndo;
        CanRedo = canRedo;
        UndoDescription = undoDescription;
        RedoDescription = redoDescription;
    }
}
