namespace LpjGuess.Core.Interfaces.Factorial;

/// <summary>
/// Interface to a class which generates factors.
/// </summary>
public interface IFactorGenerator
{
    /// <summary>
    /// The name of the factor generator. Used to generate simulation names.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Generate the factors encapsulated by this generator.
    /// </summary>
    /// <returns>The factors.</returns>
    IEnumerable<IFactor> Generate();
}
