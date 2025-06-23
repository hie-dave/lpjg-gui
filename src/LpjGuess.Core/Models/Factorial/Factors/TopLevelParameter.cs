using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Parsers;

namespace LpjGuess.Core.Models.Factorial.Factors;

/// <summary>
/// A top-level parameter which may be applied to an instruction file.
/// </summary>
public class TopLevelParameter : IFactor
{
    /// <summary>
    /// Name of the modified parameter.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// The value to be applied to the parameter.
    /// </summary>
    public string Value { get; private init; }

    /// <summary>
    /// Create a new <see cref="TopLevelParameter"/> instance.
    /// </summary>
    /// <param name="name">The name of the factor.</param>
    /// <param name="value">The value of the factor.</param>
    public TopLevelParameter(string name, string value)
    {
        Name = name;
        Value = value;
    }

    /// <inheritdoc />
    public string GetName()
    {
        return $"{Name}-{Value}";
    }

    /// <inheritdoc />
    public virtual void Apply(InstructionFileParser instructionFile)
    {
        instructionFile.SetTopLevelParameterValue(Name, Value);
    }
}
