using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Frontend.Utility;

namespace LpjGuess.Frontend.Events;

/// <summary>
/// An event parameter encapsulating a change to the style provider for a
/// particular property of a model.
/// </summary>
/// <typeparam name="TObject">The type of the changed object.</typeparam>
/// <typeparam name="TStyle">The type of the style.</typeparam>
public abstract class StyleProviderChangeEvent<TObject, TStyle> : IModelChange<TObject>
{
    /// <summary>
    /// The new style variation strategy.
    /// </summary>
    protected readonly StyleVariationStrategy strategy;

    /// <summary>
    /// Function to get the current value of the style provider.
    /// </summary>
    protected readonly Func<TObject, IStyleProvider<TStyle>> getValue;

    /// <summary>
    /// Function to set a new value for the style provider.
    /// </summary>
    protected readonly Action<TObject, IStyleProvider<TStyle>> setValue;

    /// <summary>
    /// Create a new <see cref="StyleProviderChangeEvent{TObject, TStyle}"/>
    /// instance.
    /// </summary>
    /// <param name="strategy">The new style variation strategy.</param>
    /// <param name="getValue">Function to get the current value of the style
    /// provider.</param>
    /// <param name="setValue">Function to set a new value for the style
    /// provider.</param>
    public StyleProviderChangeEvent(
        StyleVariationStrategy strategy,
        Func<TObject, IStyleProvider<TStyle>> getValue,
        Action<TObject, IStyleProvider<TStyle>> setValue)
    {
        this.strategy = strategy;
        this.getValue = getValue;
        this.setValue = setValue;
    }

    /// <inheritdoc />
    public abstract ICommand ToCommand(TObject model);
}
