namespace LpjGuess.Runner.Models;

/// <summary>
/// Represents a set of values to apply to a specific parameter.
/// </summary>
public class ParameterValues
{
    /// <summary>
    /// Name of the parameter.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// List of values to apply to the parameter.
    /// </summary>
    public List<string> Values { get; private init; }

    /// <summary>
    /// Create a new <see cref="ParameterValues"/> instance.
    /// </summary>
    /// <param name="name">Name of the parameter.</param>
    /// <param name="values">List of values to apply to the parameter.</param>
    public ParameterValues(string name, List<string> values)
    {
        Name = name;
        Values = values;
    }
}
