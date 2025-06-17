using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Parsers;

namespace LpjGuess.Core.Models.Factorial.Factors;

/// <summary>
/// A block parameter which may be applied to an instruction file.
/// </summary>
public class BlockParameter : IFactor
{
    /// <summary>
    /// Name of the modified parameter.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// Type of the block to which the parameter belongs.
    /// </summary>
    public string BlockType { get; private init; }

    /// <summary>
    /// Name of the block to which the parameter belongs.
    /// </summary>
    public string BlockName { get; private init; }

    /// <summary>
    /// The value to be applied to the parameter.
    /// </summary>
    public string Value { get; private init; }

    /// <summary>
    /// Create a new <see cref="BlockParameter"/> instance.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="blockType">The type of the block to which the parameter belongs.</param>
    /// <param name="blockName">The name of the block to which the parameter belongs.</param>
    /// <param name="value">The value to be applied to the parameter.</param>
    public BlockParameter(string name, string blockType, string blockName, string value)
    {
        Name = name;
        BlockType = blockType;
        BlockName = blockName;
        Value = value;
    }

    /// <summary>
    /// Create a new <see cref="BlockParameter"/> instance for a PFT parameter.
    /// </summary>
    /// <param name="pft">The PFT to which the parameter belongs.</param>
    /// <param name="parameter">The name of the parameter.</param>
    /// <param name="value">The value to be applied to the parameter.</param>
    /// <returns>The <see cref="BlockParameter"/> instance.</returns>
    public static BlockParameter Pft(string pft, string parameter, string value)
        => new(parameter, "pft", pft, value);

    /// <inheritdoc />
    public string GetName()
    {
        return $"{BlockName}.{Name}-{Value}";
    }

    /// <inheritdoc />
    public void Apply(InstructionFileParser instructionFile)
    {
        // TODO: check string handling.
        instructionFile.SetBlockParameterValue(BlockType, BlockName, Name, Value);
    }
}
