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
                return new TopLevelParameter("wateruptake", "wcont");
            case FactorType.Block:
                return new BlockParameter("sla", "pft", "TeBE", "30");
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
                return "Override a single top-level parameter (e.g. wateruptake)";
            case FactorType.Block:
                return "Override a single block (e.g. PFT)-level parameter (e.g. sla)";
            case FactorType.Composite:
                return "Change multiple parameters at once";
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
                return "Top-Level";
            case FactorType.Block:
                return "Block";
            case FactorType.Composite:
                return "Composite";
            case FactorType.Dummy:
                return "Identity";
            default:
                throw new NotImplementedException($"Unknown factor type: {type}");
        }
    }
}
