using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// A command that changes a property of an object.
/// </summary>
public class PropertyChangeCommand<TObject, TValue> : ICommand
{
    /// <summary>
    /// The object which will be modified.
    /// </summary>
    private readonly TObject target;

    /// <summary>
    /// The old value of the property.
    /// </summary>
    private readonly TValue oldValue;

    /// <summary>
    /// The new value of the property.
    /// </summary>
    private readonly TValue newValue;

    /// <summary>
    /// The action to set the property to the new value.
    /// </summary>
    private readonly Action<TObject, TValue> setValue;

    /// <summary>
    /// Create a new <see cref="PropertyChangeCommand{TObject, TValue}"/> instance.
    /// </summary>
    /// <param name="target">The object to change the property of.</param>
    /// <param name="oldValue">The old value of the property.</param>
    /// <param name="newValue">The new value of the property.</param>
    /// <param name="setValue">The action to set the property to the new value.</param>
    public PropertyChangeCommand(
        TObject target,
        TValue oldValue,
        TValue newValue,
        Action<TObject, TValue> setValue)
    {
        this.target = target;
        this.setValue = setValue;
        this.oldValue = oldValue;
        this.newValue = newValue;
    }

    /// <inheritdoc />
    public void Execute()
    {
        setValue(target, newValue);
    }

    /// <inheritdoc />
    public void Undo()
    {
        setValue(target, oldValue);
    }

    /// <inheritdoc />
    public string GetDescription()
    {
        return $"Change property {oldValue} to {newValue}";
    }
}
