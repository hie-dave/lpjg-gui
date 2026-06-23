namespace LpjGuess.Frontend.Classes;

using LpjGuess.Core.Models.Factorial;

/// <summary>
/// A change to a parameter in an instruction file.
/// </summary>
public class ParameterChange
{
    /// <summary>
    /// The fully-qualified parameter target.
    /// </summary>
    public ParameterTarget Target { get; private init; }

    /// <summary>
    /// The name of the parameter being changed
    /// </summary>
    public string ParameterName => Target.ParameterName;

    /// <summary>
    /// The fully-qualified name displayed to the user.
    /// </summary>
    public string DisplayTarget => Target.DisplayName;

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
    /// <param name="target">The parameter being changed.</param>
    /// <param name="value">The new value for the parameter</param>
    /// <param name="sourceFactor">The factor that caused this change</param>
    public ParameterChange(ParameterTarget target, string value, string sourceFactor)
    {
        Target = target;
        Value = value;
        SourceFactor = sourceFactor;
    }
}
