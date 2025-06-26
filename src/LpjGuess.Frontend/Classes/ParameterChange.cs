namespace LpjGuess.Frontend.Classes;

/// <summary>
/// A change to a parameter in an instruction file.
/// </summary>
public class ParameterChange
{
    /// <summary>
    /// The name of the parameter being changed
    /// </summary>
    public string ParameterName { get; private init; }

    /// <summary>
    /// The new value for the parameter
    /// </summary>
    public string Value { get; private init; }

    /// <summary>
    /// The factor that caused this change
    /// </summary>
    public string SourceFactor { get; private init; }

    /// <summary>
    /// Create a new <see cref="ParameterChange"/> instance.
    /// </summary>
    /// <param name="parameterName">The name of the parameter being changed</param>
    /// <param name="value">The new value for the parameter</param>
    /// <param name="sourceFactor">The factor that caused this change</param>
    public ParameterChange(string parameterName, string value, string sourceFactor)
    {
        ParameterName = parameterName;
        Value = value;
        SourceFactor = sourceFactor;
    }
}
