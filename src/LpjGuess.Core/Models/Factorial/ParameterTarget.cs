namespace LpjGuess.Core.Models.Factorial;

/// <summary>
/// Identifies a parameter in an LPJ-GUESS instruction file.
/// </summary>
public sealed record ParameterTarget
{
    /// <summary>
    /// The block type, or <see langword="null"/> for a top-level parameter.
    /// </summary>
    public string? BlockType { get; }

    /// <summary>
    /// The block name, or <see langword="null"/> for a top-level parameter.
    /// </summary>
    public string? BlockName { get; }

    /// <summary>
    /// The parameter name.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// A fully-qualified, user-facing representation of the target.
    /// </summary>
    public string DisplayName => BlockType is null
        ? ParameterName
        : $"{BlockType}[{BlockName}].{ParameterName}";

    private ParameterTarget(string? blockType, string? blockName, string parameterName)
    {
        BlockType = blockType;
        BlockName = blockName;
        ParameterName = parameterName;
    }

    /// <summary>
    /// Create a target for a top-level parameter.
    /// </summary>
    public static ParameterTarget TopLevel(string parameterName)
        => new(null, null, parameterName);

    /// <summary>
    /// Create a target for a parameter inside a named block.
    /// </summary>
    public static ParameterTarget Block(string blockType, string blockName, string parameterName)
        => new(blockType, blockName, parameterName);

    /// <inheritdoc />
    public override string ToString() => DisplayName;
}
