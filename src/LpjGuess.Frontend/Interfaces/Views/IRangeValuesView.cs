using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view which allows the user to configure a range values
/// generator.
/// </summary>
/// <remarks>
/// Range generators are generic on the type of the values they generate.
/// Communication with the view is done primarily via strings, because that's
/// fundamentally how they will be displayed to the user anyway, so this forces
/// the presenter to handle conversions appropriately and doesn't require the
/// view to deal with type-specific logic.
/// </remarks>
public interface IRangeValuesView : IView
{
    /// <summary>
    /// Called when the user has changed the start value.
    /// </summary>
    Event<string> OnStartChanged { get; }

    /// <summary>
    /// Called when the user has changed the number of values.
    /// </summary>
    Event<int> OnNChanged { get; }

    /// <summary>
    /// Called when the user has changed the step value.
    /// </summary>
    Event<string> OnStepChanged { get; }

    /// <summary>
    /// Populate the view with the given values.
    /// </summary>
    /// <param name="start">The start value.</param>
    /// <param name="n">The number of values.</param>
    /// <param name="step">The step value.</param>
    /// <param name="values">The values which would be generated, to be displayed as a hint.</param>
    void Populate(string start, int n, string step, IEnumerable<string> values);

    /// <summary>
    /// Set the indicator for whether there are more values than can be displayed.
    /// </summary>
    /// <param name="moreValues">True if there are more values than can be displayed, false otherwise.</param>
    void SetMoreValuesIndicator(bool moreValues);
}
