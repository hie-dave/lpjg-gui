using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a top-level factor generator view.
/// </summary>
public interface IBlockFactorGeneratorView : IView
{
    /// <summary>
    /// Event which is raised when the top-level factor generator changes.
    /// </summary>
    Event<IModelChange<BlockFactorGenerator>> OnChanged { get; }

    /// <summary>
    /// Event which is raised when the user has clicked the "Add Value" button.
    /// </summary>
    Event OnAddValue { get; }

    /// <summary>
    /// Populate the view with the given top-level factor generator.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="blockType">The type of the block to which the parameter belongs.</param>
    /// <param name="blockName">The name of the block to which the parameter belongs.</param>
    /// <param name="values">The values to be applied to the parameter.</param>
    void Populate(string name, string blockType, string blockName, List<string> values);
}
