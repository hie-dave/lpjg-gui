namespace LpjGuess.Core.Models.Factorial;

/// <summary>
/// A concrete value to apply to an instruction-file parameter.
/// </summary>
public sealed record ParameterOverride(ParameterTarget Target, string Value);
