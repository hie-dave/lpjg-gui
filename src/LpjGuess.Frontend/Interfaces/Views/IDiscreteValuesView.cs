using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view which displays a list of discrete values.
/// </summary>
public interface IDiscreteValuesView : IView
{
    /// <summary>
    /// Called when the values are changed by the user. Event parameter are the
    /// new values.
    /// </summary>
    Event<IEnumerable<string>> OnChanged { get; }

    /// <summary>
    /// Called when the user has clicked the "Add Value" button.
    /// </summary>
    Event OnAddValue { get; }

    /// <summary>
    /// Called when the user has clicked the "Remove Value" button. Event parameter
    /// is the index of the value to remove.
    /// </summary>
    Event<int> OnRemoveValue { get; }

    /// <summary>
    /// Populate the view with the given values.
    /// </summary>
    /// <param name="values">The values to display.</param>
    void Populate(IEnumerable<string> values);
}
