using LpjGuess.Core.Interfaces.Factorial;

namespace LpjGuess.Core.Models.Factorial.Generators.Factors;

/// <summary>
/// A factor generator which uses a hard-coded set of factor levels.
/// </summary>
public class SimpleFactorGenerator : IFactorGenerator
{
    /// <summary>
    /// The name of the factor generator. Used to generate simulation names.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The factor levels encapsulated by this generator.
    /// </summary>
    public IEnumerable<IFactor> Levels { get; set; }

    /// <summary>
    /// Create a new <see cref="SimpleFactorGenerator"/> instance.
    /// </summary>
    /// <param name="name">The name of the factor generator. Used to generate simulation names.</param>
    /// <param name="factors">The factors used to generate the factors.</param>
    public SimpleFactorGenerator(string name, IEnumerable<IFactor> factors)
    {
        Name = name;
        Levels = factors;
    }

    /// <inheritdoc />
    public IEnumerable<IFactor> Generate()
    {
        return Levels;
    }
}
