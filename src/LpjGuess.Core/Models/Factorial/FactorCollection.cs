using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Core.Models.Factorial;

/// <summary>
/// A collection of factors.
/// </summary>
public class FactorCollection : IFactors
{
    /// <inheritdoc />
    public string Name { get; private init; }

    /// <inheritdoc />
    public IEnumerable<IFactor> Changes { get; private init; }

    /// <summary>
    /// Create a new <see cref="Changes"/> instance.
    /// </summary>
    /// <param name="name">The name of the factors.</param>
    /// <param name="factors">The factors.</param>
    public FactorCollection(string name, IEnumerable<IFactor> factors)
    {
        Name = name;
        Changes = factors;
    }

    /// <summary>
    /// Create a new <see cref="Changes"/> instance.
    /// </summary>
    /// <param name="factors">The factors.</param>
    public FactorCollection(IEnumerable<IFactor> factors)
        : this(factors.Select(f => f.GetName()).Aggregate((x, y) => $"{x}_{y}"), factors) { }
}
