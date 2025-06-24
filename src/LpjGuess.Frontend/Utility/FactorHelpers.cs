using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Utility;

/// <summary>
/// The types of factors supported by this presenter.
/// </summary>
internal enum FactorType
{
    TopLevel,
    Block,
    Composite
}

/// <summary>
/// Utility methods for factors.
/// </summary>
internal static class FactorHelpers
{
    /// <summary>
    /// Create a factor presenter for the given factor.
    /// </summary>
    /// <param name="factor">The factor to create a presenter for.</param>
    /// <returns>The presenter.</returns>
    public static IFactorPresenter CreateFactorPresenter(IFactor factor)
    {
        // Note: BlockParameter extends TopLevelParameter, so we need to check
        // for that first.
        if (factor is BlockParameter blockParameter)
        {
            IBlockParameterView view = new BlockParameterView();
            return new BlockParameterPresenter(blockParameter, view);
        }
        if (factor is TopLevelParameter topLevelParameter)
        {
            ITopLevelParameterView view = new TopLevelParameterView();
            return new TopLevelParameterPresenter(topLevelParameter, view);
        }
        if (factor is CompositeFactor compositeFactor)
        {
            ICompositeFactorView view = new CompositeFactorView();
            return new CompositeFactorPresenter(compositeFactor, view);
        }
        throw new NotImplementedException($"Unknown factor type: {factor.GetType().Name}");
    }

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
            default:
                throw new NotImplementedException($"Unknown factor type: {type}");
        }
    }
}