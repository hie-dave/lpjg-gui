using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Parsers;

namespace LpjGuess.Core.Models.Factorial.Factors;

/// <summary>
/// This is collection of factors which are applied as a single unit.
/// </summary>
public class CompositeFactor : IFactor
{
    /// <summary>
    /// List of factors in this composite factor.
    /// </summary>
    public IEnumerable<IFactor> Factors { get; set; }

    /// <summary>
    /// Create a new <see cref="CompositeFactor"/> instance.
    /// </summary>
    /// <param name="factors">List of factors in this composite factor.</param>
    public CompositeFactor(IEnumerable<IFactor> factors)
    {
        // TODO: should we attempt to flatten this? E.g. if any of these factors
        // are themselves composite factors, we could just insert their factors
        // into this list in their place.
        Factors = factors.ToList();
    }

    /// <inheritdoc />
    public string GetName()
    {
        if (!Factors.Any())
            return string.Empty;
        return Factors.Select(f => f.GetName()).Aggregate((x, y) => $"{x}_{y}");
    }

    /// <inheritdoc />
    public void Apply(InstructionFileParser instructionFile)
    {
        foreach (IFactor factor in Factors)
            factor.Apply(instructionFile);
    }
}
