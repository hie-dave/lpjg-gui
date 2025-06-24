using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view for a concrete block factor.
/// </summary>
public interface IBlockParameterView : ITopLevelParameterView
{
    /// <summary>
    /// Event which is raised when the block factor changes.
    /// </summary>
    new Event<IModelChange<BlockParameter>> OnChanged { get; }

    /// <summary>
    /// Populate the view with the given block factor.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="blockType">The type of the block.</param>
    /// <param name="blockName">The name of the block.</param>
    /// <param name="value">The value of the parameter.</param>
    void Populate(string name, string blockType, string blockName, string value);
}
