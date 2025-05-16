using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Events;

/// <summary>
/// Encapsulates a change to a model object made by the user.
/// </summary>
/// <typeparam name="TObject">The type of the changed object.</typeparam>
/// <typeparam name="TValue">The type of the changed value.</typeparam>
public class ModelChangeEventArgs<TObject, TValue> : IModelChange<TObject>
{
    /// <summary>
    /// Function to get the current value of the property.
    /// </summary>
    public Func<TObject, TValue> GetValue { get; }

    /// <summary>
    /// Function to set a new value for the property.
    /// </summary>
    public Action<TObject, TValue> SetValue { get; }

    /// <summary>
    /// The new value from the UI control.
    /// </summary>
    public TValue NewValue { get; }

    /// <summary>
    /// Create a new <see cref="ModelChangeEventArgs{TObject, TValue}"/> instance.
    /// </summary>
    /// <param name="getValue">Function to get the current value.</param>
    /// <param name="setValue">Function to set a new value.</param>
    /// <param name="newValue">The new value from the UI control.</param>
    public ModelChangeEventArgs(
        Func<TObject, TValue> getValue,
        Action<TObject, TValue> setValue,
        TValue newValue)
    {
        GetValue = getValue;
        SetValue = setValue;
        NewValue = newValue;
    }

    /// <inheritdoc />
    public ICommand ToCommand(TObject model)
    {
        return new PropertyChangeCommand<TObject, TValue>(
            model,
            GetValue(model),
            NewValue,
            SetValue);
    }
}
