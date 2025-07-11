namespace LpjGuess.Runner.Models;

/// <summary>
/// Represents a group of parameters that should be varied together, ensuring that values at the same
/// index are used together in simulations. This is useful for parameters that are logically related,
/// such as different climate variables from the same dataset.
/// </summary>
public class ParameterGroup
{
    /// <summary>
    /// Name of the parameter group, used for identification and logging.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// List of parameters in this group.
    /// All parameters in a group must have the same number of values.
    /// </summary>
    public IReadOnlyList<ParameterValues> Parameters { get; private init; }

    /// <summary>
    /// Create a new <see cref="ParameterGroup"/> instance.
    /// </summary>
    /// <param name="name">Name of the parameter group.</param>
    /// <param name="parameters">Dictionary of parameter names and their values.</param>
    /// <exception cref="InvalidOperationException">Thrown if parameters have different numbers of values.</exception>
    public ParameterGroup(string name, IReadOnlyList<ParameterValues> parameters)
    {
        ValidateParameters(parameters);
        Name = name;
        Parameters = parameters;
    }

    /// <summary>
    /// Gets the number of values for each parameter in this group.
    /// All parameters in a group must have the same number of values.
    /// </summary>
    public int ValueCount => Parameters.First().Values.Count;

    /// <summary>
    /// Get the factors for a specific value index across all parameters in the group.
    /// </summary>
    /// <param name="valueIndex">Index of the value to use for each parameter.</param>
    /// <returns>List of factors using values at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if valueIndex is out of range.</exception>
    public IReadOnlyList<Factor> GetFactorsForIndex(int valueIndex)
    {
        if (valueIndex < 0 || valueIndex >= ValueCount)
            throw new ArgumentOutOfRangeException(nameof(valueIndex), "Value index must be within range of parameter values");

        return Parameters
            .Select(p => new Factor(p.Name, p.Values[valueIndex]))
            .ToList();
    }

    private void ValidateParameters(IReadOnlyList<ParameterValues> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            throw new ArgumentException("Parameter group must contain at least one parameter", nameof(parameters));

        int expectedCount = parameters.First().Values.Count;
        if (expectedCount == 0)
            throw new ArgumentException("Parameters must have at least one value", nameof(parameters));

        foreach (var param in parameters)
        {
            if (param.Values.Count != expectedCount)
                throw new InvalidOperationException(
                    $"All parameters in a group must have the same number of values. " +
                    $"Parameter '{param.Name}' has {param.Values.Count} values, expected {expectedCount}.");
        }
    }
}
