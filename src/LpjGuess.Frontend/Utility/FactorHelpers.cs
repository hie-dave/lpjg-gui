using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;

namespace LpjGuess.Frontend.Utility;

/// <summary>
/// The types of factors supported by this presenter.
/// </summary>
internal enum FactorType
{
    TopLevel,
    Block,
    Composite,
    Dummy
}

/// <summary>
/// Utility methods for factors.
/// </summary>
internal static class FactorHelpers
{
    /// <summary>
    /// Create a default factor for the given factor type.
    /// </summary>
    /// <param name="type">The type of factor to create.</param>
    /// <returns>The default factor.</returns>
    /// <exception cref="NotImplementedException">Thrown if the factor type is unknown.</exception>
    public static IFactor CreateDefaultFactor(FactorType type)
    {
        switch (type)
        {
            case FactorType.TopLevel:
                return new TopLevelParameter(string.Empty, string.Empty);
            case FactorType.Block:
                return new BlockParameter("pft", string.Empty, string.Empty, string.Empty);
            case FactorType.Composite:
                return new CompositeFactor([]);
            case FactorType.Dummy:
                return new DummyFactor();
            default:
                throw new NotImplementedException($"Unknown factor type: {type}");
        }
    }

    /// <summary>
    /// Get a description of the given factor type.
    /// </summary>
    /// <param name="type">The factor type to get a description for.</param>
    /// <returns>The description of the factor type.</returns>
    public static string GetFactorTypeDescription(FactorType type)
    {
        switch (type)
        {
            case FactorType.TopLevel:
                return "Change one global instruction-file parameter";
            case FactorType.Block:
                return "Change one parameter inside a named PFT, stand, or other block";
            case FactorType.Composite:
                return "Group several parameter changes into one scenario";
            case FactorType.Dummy:
                return "Make no changes";
            default:
                throw new NotImplementedException($"Unknown factor type: {type}");
        }
    }

    /// <summary>
    /// Get the name of the given factor type.
    /// </summary>
    /// <param name="type">The factor type to get the name for.</param>
    /// <returns>The name of the factor type.</returns>
    public static string GetFactorTypeName(FactorType type)
    {
        switch (type)
        {
            case FactorType.TopLevel:
                return "Global parameter";
            case FactorType.Block:
                return "Block parameter";
            case FactorType.Composite:
                return "Multi-parameter scenario";
            case FactorType.Dummy:
                return "Identity";
            default:
                throw new NotImplementedException($"Unknown factor type: {type}");
        }
    }
}
