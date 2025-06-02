namespace LpjGuess.Core.Interfaces.Graphing.Style;

/// <summary>
/// An interface to a class which maps a numeric index to a style value.
/// </summary>
public interface IStyleStrategy<T>
{
    /// <summary>
    /// Get the value for the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The value.</returns>
    T GetValue(uint index);

    /// <summary>
    /// Initialise the style strategy by telling it the total number of styles
    /// which will be required.
    /// </summary>
    void Initialise(int totalStylesCount);
}
