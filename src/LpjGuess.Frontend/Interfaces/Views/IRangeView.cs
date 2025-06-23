using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view which displays a range value generator.
/// </summary>
public interface IRangeView : IView
{
    /// <summary>
    /// Called when the start value is changed by the user.
    /// </summary>
    Event<string> OnStartChanged { get; }

    /// <summary>
    /// Called when the number of values is changed by the user.
    /// </summary>
    Event<int> OnNChanged { get; }

    /// <summary>
    /// Called when the step size is changed by the user.
    /// </summary>
    Event<string> OnStepChanged { get; }

    /// <summary>
    /// Populate the view with the given range generator.
    /// </summary>
    /// <param name="start">The start value.</param>
    /// <param name="n">The number of values.</param>
    /// <param name="step">The step size.</param>
    void Populate(string start, int n, string step);
}
