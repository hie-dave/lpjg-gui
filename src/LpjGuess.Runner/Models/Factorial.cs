namespace LpjGuess.Runner.Models;

/// <summary>
/// This class encapsulates a list of changed .ins file parameters to be applied
/// to an LPJ-Guess simulation.
/// </summary>
public class Factorial
{
    /// <summary>
    /// Parameter changes to be applied to an LPJ-Guess simulation.
    /// </summary>
    public IReadOnlyCollection<Factor> Factors { get; private init; }

    /// <summary>
    /// Optional parameter groups that are part of this factorial.
    /// Parameter groups ensure that values at matching indices are used together.
    /// </summary>
    public IReadOnlyCollection<ParameterGroup> ParameterGroups { get; private init; }

    /// <summary>
    /// Create a new <see cref="Factorial"/> instance.
    /// </summary>
    /// <param name="factors">Parameter changes to be applied to an LPJ-Guess simulation.</param>
    /// <param name="parameterGroups">Optional parameter groups to be included in this factorial.</param>
    public Factorial(IReadOnlyCollection<Factor> factors, IReadOnlyCollection<ParameterGroup>? parameterGroups = null)
    {
        Factors = factors;
        ParameterGroups = parameterGroups ?? Array.Empty<ParameterGroup>();
    }

    /// <summary>
    /// Get a name which describes the factorial.
    /// </summary>
    public string GetName()
    {
        if (Factors.Count == 0 && ParameterGroups.Count == 0)
            return "base";

        var allFactors = Factors.ToList();
        foreach (var group in ParameterGroups)
        {
            allFactors.AddRange(group.GetFactorsForIndex(0)); // Use first value for name
        }

        return string.Join("-", allFactors.Select(f => f.GetName()));
    }
}
