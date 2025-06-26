using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Interfaces.Commands;
using Xunit;

namespace LpjGuess.Tests.Commands;

public class CommandRegistryTests
{
    private class TestCommand : ICommand
    {
        public bool WasExecuted { get; private set; }
        public bool WasUndone { get; private set; }
        public string Description { get; }

        public TestCommand(string description = "Test Command")
        {
            Description = description;
        }

        public void Execute()
        {
            WasExecuted = true;
            WasUndone = false;
        }

        public void Undo()
        {
            WasUndone = true;
            WasExecuted = false;
        }

        public string GetDescription()
        {
            return Description;
        }
    }

    [Fact]
    public void Execute_ShouldExecuteCommand()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        var command = new TestCommand();

        registry.Execute(command);

        Assert.True(command.WasExecuted);
        Assert.False(command.WasUndone);
    }

    [Fact]
    public void Execute_ShouldAddCommandToHistory()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        var command = new TestCommand();

        registry.Execute(command);

        Assert.True(registry.CanUndo);
        Assert.False(registry.CanRedo);
    }

    [Fact]
    public void Undo_ShouldUndoLastCommand()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        var command = new TestCommand();
        registry.Execute(command);

        bool result = registry.Undo();

        Assert.True(result);
        Assert.True(command.WasUndone);
        Assert.False(command.WasExecuted);
    }

    [Fact]
    public void Undo_ShouldReturnFalseWhenNoCommandsToUndo()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state

        bool result = registry.Undo();

        Assert.False(result);
    }

    [Fact]
    public void Redo_ShouldRedoLastUndoneCommand()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        var command = new TestCommand();
        registry.Execute(command);
        registry.Undo();

        bool result = registry.Redo();

        Assert.True(result);
        Assert.True(command.WasExecuted);
        Assert.False(command.WasUndone);
    }

    [Fact]
    public void Redo_ShouldReturnFalseWhenNoCommandsToRedo()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state

        bool result = registry.Redo();

        Assert.False(result);
    }

    [Fact]
    public void Execute_ShouldClearRedoHistoryWhenExecutingNewCommand()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        var command1 = new TestCommand("Command 1");
        var command2 = new TestCommand("Command 2");
        registry.Execute(command1);
        registry.Undo();
        
        registry.Execute(command2);
        
        Assert.True(registry.CanUndo);
        Assert.False(registry.CanRedo);
    }

    [Fact]
    public void ClearHistory_ShouldRemoveAllCommands()
    {
        var registry = CommandRegistry.Instance;
        registry.Execute(new TestCommand());
        
        registry.ClearHistory();
        
        Assert.False(registry.CanUndo);
        Assert.False(registry.CanRedo);
    }

    [Fact]
    public void Events_ShouldFireBeforeAndAfterCommandExecution()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        var command = new TestCommand();
        bool beforeFired = false;
        bool afterFired = false;
        
        var beforeHandler = new Action<ICommand>(cmd => beforeFired = true);
        var afterHandler = new Action<ICommand>(cmd => afterFired = true);
        
        registry.BeforeCommandExecuted.ConnectTo(beforeHandler);
        registry.AfterCommandExecuted.ConnectTo(afterHandler);
        
        try
        {
            registry.Execute(command);
            
            Assert.True(beforeFired);
            Assert.True(afterFired);
        }
        finally
        {
            // Cleanup
            registry.BeforeCommandExecuted.DisconnectFrom(beforeHandler);
            registry.AfterCommandExecuted.DisconnectFrom(afterHandler);
        }
    }

    [Fact]
    public void Events_ShouldFireBeforeAndAfterCommandUndo()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        var command = new TestCommand();
        registry.Execute(command);
        
        bool beforeFired = false;
        bool afterFired = false;
        
        var beforeHandler = new Action<ICommand>(cmd => beforeFired = true);
        var afterHandler = new Action<ICommand>(cmd => afterFired = true);
        
        registry.BeforeCommandUndone.ConnectTo(beforeHandler);
        registry.AfterCommandUndone.ConnectTo(afterHandler);
        
        try
        {
            registry.Undo();
            
            Assert.True(beforeFired);
            Assert.True(afterFired);
        }
        finally
        {
            // Cleanup
            registry.BeforeCommandUndone.DisconnectFrom(beforeHandler);
            registry.AfterCommandUndone.DisconnectFrom(afterHandler);
        }
    }

    [Fact]
    public void Events_ShouldFireHistoryStateChangedEvent()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        var command = new TestCommand();
        bool eventFired = false;
        CommandHistoryState? capturedState = null;
        
        var handler = new Action<CommandHistoryState>(state => {
            eventFired = true;
            capturedState = state;
        });
        
        registry.HistoryStateChanged.ConnectTo(handler);
        
        try
        {
            registry.Execute(command);
            
            Assert.True(eventFired);
            Assert.NotNull(capturedState);
            Assert.True(capturedState.CanUndo);
            Assert.False(capturedState.CanRedo);
            Assert.Equal(command.GetDescription(), capturedState.UndoDescription);
        }
        finally
        {
            // Cleanup
            registry.HistoryStateChanged.DisconnectFrom(handler);
        }
    }

    [Fact]
    public void GetHistoryState_ShouldReturnCorrectState()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        var command1 = new TestCommand("Command 1");
        var command2 = new TestCommand("Command 2");
        
        registry.Execute(command1);
        registry.Execute(command2);
        registry.Undo();
        var state = registry.GetHistoryState();
        
        Assert.True(state.CanUndo);
        Assert.True(state.CanRedo);
        Assert.Equal(command1.GetDescription(), state.UndoDescription);
        Assert.Equal(command2.GetDescription(), state.RedoDescription);
    }

    [Fact]
    public void CommandRegistry_ShouldBeSingleton()
    {
        // Arrange & Act
        var instance1 = CommandRegistry.Instance;
        var instance2 = CommandRegistry.Instance;
        
        Assert.Same(instance1, instance2);
    }
    
    [Fact]
    public void MultipleCommands_ShouldBeUndoneInReverseOrder()
    {
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        var commands = new List<TestCommand>
        {
            new TestCommand("Command 1"),
            new TestCommand("Command 2"),
            new TestCommand("Command 3")
        };
        
        // Execute all commands
        foreach (var command in commands)
        {
            registry.Execute(command);
        }
        
        // Act & Assert - Undo in reverse order
        for (int i = commands.Count - 1; i >= 0; i--)
        {
            registry.Undo();
            Assert.True(commands[i].WasUndone);
            
            // Verify earlier commands are still in executed state
            for (int j = 0; j < i; j++)
            {
                Assert.True(commands[j].WasExecuted);
                Assert.False(commands[j].WasUndone);
            }
        }
    }
    
    [Fact]
    public void MaxHistorySize_ShouldRemoveOldestCommandsWhenExceeded()
    {
        // Arrange
        var registry = CommandRegistry.Instance;
        registry.ClearHistory(); // Start with a clean state
        
        // The default maxHistorySize in CommandRegistry is 100
        // Create 105 commands to exceed this limit
        const int commandCount = 105;
        var commands = new List<TestCommand>();
        
        // Act - Execute more commands than the max history size
        for (int i = 0; i < commandCount; i++)
        {
            var command = new TestCommand($"Command {i}");
            commands.Add(command);
            registry.Execute(command);
        }
        
        // Assert
        
        // 1. We should be able to undo the most recent commands
        for (int i = commandCount - 1; i >= commandCount - 10; i--)
        {
            Assert.True(registry.CanUndo);
            registry.Undo();
            Assert.True(commands[i].WasUndone);
        }
        
        // 2. We should still be able to undo more commands
        Assert.True(registry.CanUndo);
        
        // 3. But we shouldn't be able to undo all the way back to the first commands
        // Undo the remaining commands that should be in history
        int undoCount = 0;
        while (registry.CanUndo)
        {
            registry.Undo();
            undoCount++;
        }
        
        // We should have been able to undo at most 100 commands total (the default max history size)
        Assert.Equal(90, undoCount); // 10 already undone + 90 more = 100 total
        
        // 4. The oldest commands should not have been undone because they were removed from history
        for (int i = 0; i < commandCount - 100; i++)
        {
            Assert.False(commands[i].WasUndone);
            Assert.True(commands[i].WasExecuted);
        }
    }
}
