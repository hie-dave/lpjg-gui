using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// A command which executes a sequence of other commands.
/// </summary>
/// <remarks>
/// Commands are guaranteed to be executed in the order they are provided to the constructor.
/// </remarks>
public class CompositeCommand : ICommand
{
    /// <summary>
    /// The commands to execute.
    /// </summary>
    private readonly IReadOnlyList<ICommand> commands;

    /// <summary>
    /// Create a new <see cref="CompositeCommand"/> instance.
    /// </summary>
    /// <param name="commands">The commands to execute.</param>
    public CompositeCommand(IEnumerable<ICommand> commands)
    {
        this.commands = commands.ToList();
    }

    /// <inheritdoc />
    public void Execute()
    {
        foreach (ICommand command in commands)
            command.Execute();
    }

    /// <inheritdoc />
    public void Undo()
    {
        // Undo commands in reverse order
        for (int i = commands.Count - 1; i >= 0; i--)
        {
            commands[i].Undo();
        }
    }
}
