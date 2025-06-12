using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identifiers;
using LpjGuess.Core.Models.Graphing.Style.Providers;
using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// A command to change the style provider of a model's style property.
/// </summary>
/// <typeparam name="TObject">The model type.</typeparam>
/// <typeparam name="TStyle">The style type.</typeparam>
public abstract class StyleProviderChangeCommandBase<TObject, TStyle> : ICommand where TStyle : struct
{
    /// <summary>
    /// The target object.
    /// </summary>
    private readonly TObject target;

    /// <summary>
    /// The old value of the style provider.
    /// </summary>
    private readonly IStyleProvider<TStyle> oldValue;

    /// <summary>
    /// The new value of the style provider.
    /// </summary>
    private readonly IStyleProvider<TStyle> newValue;

    /// <summary>
    /// The action to set the style provider to the new value.
    /// </summary>
    private readonly Action<TObject, IStyleProvider<TStyle>> setValue;

    /// <summary>
    /// Create a new <see cref="StyleProviderChangeCommandBase{TObject, TStyle}"/>
    /// instance.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="strategy">The style variation strategy.</param>
    /// <param name="getValue">The function to get the current value.</param>
    /// <param name="setValue">The action to set the style provider to the new
    /// value.</param>
    public StyleProviderChangeCommandBase(
        TObject target,
        StyleVariationStrategy strategy,
        Func<TObject, IStyleProvider<TStyle>> getValue,
        Action<TObject, IStyleProvider<TStyle>> setValue)
    {
        this.target = target;
        oldValue = getValue(target);
        newValue = CreateStyleProvider(strategy);
        this.setValue = setValue;
    }

    /// <inheritdoc />
    public void Execute()
    {
        setValue(target, newValue);
    }

    /// <inheritdoc />
    public void Undo()
    {
        setValue(target, oldValue);
    }

    /// <summary>
    /// Create a style strategy corresponding suitable for the style type.
    /// </summary>
    /// <returns>The style strategy.</returns>
    protected abstract IStyleStrategy<TStyle> CreateStrategy();

    /// <summary>
    /// Create a style provider corresponding to the specified style variation
    /// strategy.
    /// </summary>
    /// <param name="strategy">The style variation strategy.</param>
    /// <returns>The style provider.</returns>
    private IStyleProvider<TStyle> CreateStyleProvider(StyleVariationStrategy strategy)
    {
        // This class doesn't have the context (ie a TStyle instance) to create
        // a fixed provider, so this will throw if strategy is fixed.
        if (strategy == StyleVariationStrategy.Fixed)
            return new FixedStyleProvider<TStyle>(DefaultStyle());

        ISeriesIdentifier identifier = CreateIdentifier(strategy);
        IStyleStrategy<TStyle> valueStrategy = CreateStrategy();
        return new DynamicStyleProvider<TStyle>(identifier, valueStrategy);
    }

    /// <summary>
    /// Get the default style value used when the user has changed the property
    /// to a "fixed" strategy.
    /// </summary>
    /// <returns>The default style.</returns>
    protected virtual TStyle DefaultStyle() => default;

    /// <summary>
    /// Create a series identifier corresponding to the specified style
    /// variation strategy.
    /// </summary>
    /// <param name="variation">The style variation strategy.</param>
    /// <returns>The series identifier.</returns>
    /// <exception cref="ArgumentException"></exception>
    private static ISeriesIdentifier CreateIdentifier(StyleVariationStrategy variation)
    {
        return variation switch
        {
            StyleVariationStrategy.ByGridcell => new GridcellIdentifier(),
            StyleVariationStrategy.BySimulation => new SimulationIdentifier(),
            StyleVariationStrategy.ByStand => new StandIdentifier(),
            StyleVariationStrategy.ByPatch => new PatchIdentifier(),
            StyleVariationStrategy.ByIndividual => new IndividualIdentifier(),
            StyleVariationStrategy.ByPft => new PftIdentifier(),
            StyleVariationStrategy.BySeries => new SeriesIdentifier(),
            StyleVariationStrategy.ByLayer => new LayerIdentifier(),
            _ => throw new ArgumentException($"Invalid style variation strategy: {variation}")
        };
    }
}
