namespace LpjGuess.Core.Models.Factorial;

/// <summary>
/// The type of value generator to use.
/// </summary>
public enum ValueGeneratorType
{
    /// <summary>
    /// Use a fixed list of values.
    /// </summary>
    Discrete,

    /// <summary>
    /// Generate a range of values from a predefined start/stop/step.
    /// </summary>
    Range
}
