using System.Globalization;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;

namespace LpjGuess.Core.Models.Factorial.Generators.Factors;

/// <summary>
/// A simple block factor generator which generates a set of factors for a single
/// block parameter.
/// </summary>
public class BlockFactorGenerator : TopLevelFactorGenerator
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
    /// Create a new <see cref="BlockFactorGenerator"/> instance.
    /// </summary>
    /// <param name="blockType">The type of the block to which the parameter belongs.</param>
    /// <param name="blockName">The name of the block to which the parameter belongs.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="values">The value generator used to generate the values to be applied to the parameter.</param>
    public BlockFactorGenerator(string blockType, string blockName, string name, IValueGenerator values)
        : base(name, values)
    {
        BlockType = blockType;
        BlockName = blockName;
    }

    /// <inheritdoc />
    public override IEnumerable<IFactor> Generate()
    {
        return Values
            .GenerateStrings(CultureInfo.InvariantCulture)
            .Select(v => new BlockParameter(Name, BlockType, BlockName, v));
    }
}
