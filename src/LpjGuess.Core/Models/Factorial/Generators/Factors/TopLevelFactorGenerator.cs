using System.Globalization;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;

namespace LpjGuess.Core.Models.Factorial.Generators.Factors;

/// <summary>
/// A simple factor generator which generates a set of factors for a single top-
/// level parameter.
/// </summary>
public class TopLevelFactorGenerator : IFactorGenerator
{
    /// <summary>
    /// Name of the modified parameter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The values to be applied to the parameter.
    /// </summary>
    public IValueGenerator Values { get; set; }

    /// <summary>
    /// Create a new <see cref="TopLevelFactorGenerator"/> instance.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="values">The values to be applied to the parameter.</param>
    public TopLevelFactorGenerator(string name, IValueGenerator values)
    {
        Name = name;
        Values = values;
    }

    /// <inheritdoc />
    public virtual IEnumerable<IFactor> Generate()
    {
        // Need to use invariant culture for string conversions, to ensure that
        // (for example) decimal places are rendered as periods, not commas, as
        // required by LPJ-Guess.
        return Values
            .GenerateStrings(CultureInfo.InvariantCulture)
            .Select(v => new TopLevelParameter(Name, v));
    }

    /// <inheritdoc />
    public int NumFactors() => Values.NumValues();
}
