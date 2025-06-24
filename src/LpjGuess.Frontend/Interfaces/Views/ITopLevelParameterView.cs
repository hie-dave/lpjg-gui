using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Interface to a view for a concrete top-level parameter.
/// </summary>
public interface ITopLevelParameterView : IView
{
    /// <summary>
    /// Event which is raised when the top-level parameter changes.
    /// </summary>
    Event<IModelChange<TopLevelParameter>> OnChanged { get; }

    /// <summary>
    /// Populate the view with the given top-level parameter.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    void Populate(string name, string value);
}
