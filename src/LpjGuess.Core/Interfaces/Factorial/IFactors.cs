namespace LpjGuess.Core.Interfaces.Factorial;

/// <summary>
/// Interface to a named collection of factors. This represents a discrete
/// simulation, as a set of changes to be applied to a base simulation.
/// </summary>
public interface IFactors
{
    /// <summary>
    /// Name of this collection of factors.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// List of factors in this collection.
    /// </summary>
    IEnumerable<IFactor> Changes { get; }    
}
