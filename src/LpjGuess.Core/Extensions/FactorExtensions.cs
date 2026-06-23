using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial.Generators.Factors;

namespace LpjGuess.Core.Extensions;

/// <summary>
/// Extensions for describing factors without losing block identity.
/// </summary>
public static class FactorExtensions
{
    /// <summary>
    /// Get the fully-qualified parameter overrides made by a factor.
    /// </summary>
    public static IEnumerable<ParameterOverride> GetParameterOverrides(this IFactor factor)
    {
        return factor switch
        {
            BlockParameter block =>
                [new ParameterOverride(
                    ParameterTarget.Block(block.BlockType, block.BlockName, block.Name),
                    block.Value)],
            TopLevelParameter parameter =>
                [new ParameterOverride(ParameterTarget.TopLevel(parameter.Name), parameter.Value)],
            CompositeFactor composite =>
                composite.Factors.SelectMany(GetParameterOverrides),
            DummyFactor => [],
            _ => factor.GetChanges()
                .Select(change => new ParameterOverride(
                    ParameterTarget.TopLevel(change.Item1),
                    change.Item2))
        };
    }

    /// <summary>
    /// Get all parameter targets which may be changed by a factor generator.
    /// </summary>
    public static IEnumerable<ParameterTarget> GetParameterTargets(
        this IFactorGenerator generator)
    {
        return generator switch
        {
            BlockFactorGenerator block =>
                [ParameterTarget.Block(block.BlockType, block.BlockName, block.Name)],
            TopLevelFactorGenerator parameter =>
                [ParameterTarget.TopLevel(parameter.Name)],
            SimpleFactorGenerator scenarios =>
                scenarios.Levels
                    .SelectMany(level => level.GetParameterOverrides())
                    .Select(change => change.Target)
                    .Distinct(),
            _ => generator.Generate()
                .SelectMany(factor => factor.GetParameterOverrides())
                .Select(change => change.Target)
                .Distinct()
        };
    }
}
