using LpjGuess.Core.Extensions;
using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Core.Models.Factorial.Generators;

/// <summary>
/// A simulation generator which generates a full factorial of the specified parameters.
/// </summary>
public class FactorialGenerator : ISimulationGenerator
{
    /// <summary>
    /// Whether to generate a full factorial.
    /// </summary>
    public bool FullFactorial { get; set; }

    /// <summary>
    /// The factors used to generate the simulation changes.
    /// </summary>
    public IEnumerable<IFactorGenerator> Factors { get; set; }

    /// <summary>
    /// Create a new <see cref="FactorialGenerator"/> instance.
    /// </summary>
    /// <param name="fullFactorial">Whether to generate a full factorial.</param>
    /// <param name="factors">The factors used to generate the simulation changes.</param>
    public FactorialGenerator(bool fullFactorial, IEnumerable<IFactorGenerator> factors)
    {
        FullFactorial = fullFactorial;
        Factors = factors;
    }

    /// <inheritdoc />
    public IEnumerable<IFactors> Generate()
    {
        var factors = Factors.Select(f => f.Generate());
        if (FullFactorial)
            factors = factors.AllCombinations();
        return factors.Select(f => new FactorCollection(f));
    }
}
