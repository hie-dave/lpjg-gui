using System.Globalization;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Parsers;
using Newtonsoft.Json;

namespace LpjGuess.Core.Models.Factorial.Factors;

/// <summary>
/// A top-level parameter which may be applied to an instruction file.
/// </summary>
public class TopLevelParameter : IFactor
{
    /// <summary>
    /// Name of the modified parameter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The value to be applied to the parameter.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Create a new <see cref="TopLevelParameter"/> instance.
    /// </summary>
    /// <param name="name">The name of the factor.</param>
    /// <param name="value">The value of the factor.</param>
    [JsonConstructor]
    public TopLevelParameter(string name, string value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Create a new <see cref="TopLevelParameter"/> instance with an integer
    /// value.
    /// </summary>
    /// <param name="name">The name of the factor.</param>
    /// <param name="value">The integer value of the factor.</param>
    public TopLevelParameter(string name, int value) :
        this(name, value.ToString(CultureInfo.InvariantCulture)) { }

    /// <summary>
    /// Create a new <see cref="TopLevelParameter"/> instance with a double
    /// value.
    /// </summary>
    /// <param name="name">The name of the factor.</param>
    /// <param name="value">The double value of the factor.</param>
    public TopLevelParameter(string name, double value) :
        this(name, value.ToString(CultureInfo.InvariantCulture)) { }

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

    /// <inheritdoc />
    public IEnumerable<(string, string)> GetChanges()
    {
        return [(Name, Value)];
    }
}
