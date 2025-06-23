using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a top-level factor generator view.
/// </summary>
public interface ITopLevelFactorGeneratorView : IView
{
    /// <summary>
    /// Event which is raised when the top-level factor generator changes.
    /// </summary>
    Event<IModelChange<TopLevelFactorGenerator>> OnChanged { get; }

    /// <summary>
    /// Event which is raised when the value generator type changes.
    /// </summary>
    Event<ValueGeneratorType> OnValuesTypeChanged { get; }

    /// <summary>
    /// Populate the view with the given top-level factor generator.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="valueGeneratorView">The view to be used to display the values generator.</param>
    void Populate(string name, IView valueGeneratorView);
}
