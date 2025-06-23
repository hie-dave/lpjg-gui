using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a block factor generator view.
/// </summary>
public interface IBlockFactorGeneratorView : ITopLevelFactorGeneratorView
{
    /// <summary>
    /// Event which is raised when the block factor generator changes.
    /// </summary>
    new Event<IModelChange<BlockFactorGenerator>> OnChanged { get; }

    /// <summary>
    /// Populate the view with the given block factor generator.
    /// </summary>
    /// <remarks>
    /// Note: the base class' populate method must be called *in addition* to
    /// this method.
    /// </remarks>
    /// <param name="blockType">The type of the block to which the parameter belongs.</param>
    /// <param name="blockName">The name of the block to which the parameter belongs.</param>
    void Populate(string blockType, string blockName);
}
