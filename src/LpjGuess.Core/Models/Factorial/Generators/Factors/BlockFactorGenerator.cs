using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;

namespace LpjGuess.Core.Models.Factorial.Generators.Factors;

/// <summary>
/// A simple block factor generator which generates a set of factors for a single
/// block parameter.
/// </summary>
public class BlockFactorGenerator : IFactorGenerator
{
    /// <summary>
    /// Type of the block to which the parameter belongs.
    /// </summary>
    public string BlockType { get; set; }

    /// <summary>
    /// Name of the block to which the parameter belongs.
    /// </summary>
    public string BlockName { get; set; }

    /// <summary>
    /// Name of the modified parameter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The values to be applied to the parameter.
    /// </summary>
    public List<string> Values { get; set; }

    /// <summary>
    /// Create a new <see cref="BlockFactorGenerator"/> instance.
    /// </summary>
    /// <param name="blockType">The type of the block to which the parameter belongs.</param>
    /// <param name="blockName">The name of the block to which the parameter belongs.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="values">The values to be applied to the parameter.</param>
    public BlockFactorGenerator(string blockType, string blockName, string name, List<string> values)
    {
        BlockType = blockType;
        BlockName = blockName;
        Name = name;
        Values = values;
    }

    /// <inheritdoc />
    public IEnumerable<IFactor> Generate()
    {
        return Values.Select(v => new BlockParameter(Name, BlockType, BlockName, v));
    }
}
