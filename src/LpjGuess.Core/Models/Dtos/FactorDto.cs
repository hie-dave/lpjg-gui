using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;

namespace LpjGuess.Core.Models.Dtos;

/// <summary>
/// DTO for serialisation of <see cref="IFactor"/>.
/// </summary>
public class FactorDto
{
    /// <summary>
    /// The name of the factor.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The value of the factor.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The name of the block to which the factor belongs.
    /// </summary>
    public string? BlockName { get; set; }

    /// <summary>
    /// The type of the block to which the factor belongs.
    /// </summary>
    public string? BlockType { get; set; }

    /// <summary>
    /// Creates a default <see cref="FactorDto"/>.
    /// </summary>
    public FactorDto()
    {
        Name = string.Empty;
        Value = string.Empty;
        BlockName = null;
        BlockType = null;
    }

    /// <summary>
    /// Creates a <see cref="IFactor"/> from this <see cref="FactorDto"/>.
    /// </summary>
    /// <returns>The <see cref="IFactor"/>.</returns>
    public IFactor ToFactor()
    {
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Value))
            return new DummyFactor();

        if (BlockName is null || BlockType is null)
            return new TopLevelParameter(Name, Value);

        return new BlockParameter(BlockType, BlockName, Name, Value);
    }

    /// <summary>
    /// Creates a <see cref="FactorDto"/> from a <see cref="IFactor"/>.
    /// </summary>
    /// <returns>The <see cref="FactorDto"/>.</returns>
    public static FactorDto FromFactor(IFactor factor)
    {
        return factor switch
        {
            BlockParameter blockParameter => FromBlockParameter(blockParameter),
            TopLevelParameter parameter => FromTopLevelParameter(parameter),
            DummyFactor _ => new FactorDto(),
            _ => throw new ArgumentException($"Unknown factor type: {factor.GetType().Name}")
        };
    }

    /// <summary>
    /// Creates a <see cref="FactorDto"/> from a <see cref="TopLevelParameter"/>.
    /// </summary>
    /// <param name="parameter">The <see cref="TopLevelParameter"/>.</param>
    /// <returns>The <see cref="FactorDto"/>.</returns>
    private static FactorDto FromTopLevelParameter(TopLevelParameter parameter)
    {
        return new FactorDto
        {
            Name = parameter.Name,
            Value = parameter.Value,
            BlockName = null,
            BlockType = null
        };
    }

    /// <summary>
    /// Creates a <see cref="FactorDto"/> from a <see cref="BlockParameter"/>.
    /// </summary>
    /// <param name="parameter">The <see cref="BlockParameter"/>.</param>
    /// <returns>The <see cref="FactorDto"/>.</returns>
    private static FactorDto FromBlockParameter(BlockParameter parameter)
    {
        return new FactorDto
        {
            Name = parameter.Name,
            Value = parameter.Value,
            BlockName = parameter.BlockName,
            BlockType = parameter.BlockType
        };
    }
}
