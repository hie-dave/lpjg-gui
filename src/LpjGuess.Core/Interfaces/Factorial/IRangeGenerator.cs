namespace LpjGuess.Core.Interfaces.Factorial;

/// <summary>
/// Interface to a range generator.
/// </summary>
public interface IRangeGenerator : IValueGenerator
{
    /// <summary>
    /// The number of values to generate.
    /// </summary>
    int N { get; set; }
}
