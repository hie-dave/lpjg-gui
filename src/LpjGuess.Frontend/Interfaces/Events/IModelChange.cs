using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Interfaces.Events;

/// <summary>
/// An interface to an event parameter encapsulating a change to a model.
/// </summary>
public interface IModelChange<TObject>
{
    /// <summary>
    /// Convert the model change to a command.
    /// </summary>
    /// <param name="model">The model to apply the change to.</param>
    /// <returns>The command to perform the change.</returns>
    ICommand ToCommand(TObject model);
}
