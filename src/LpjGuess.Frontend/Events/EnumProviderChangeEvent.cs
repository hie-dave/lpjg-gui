using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Events;

/// <summary>
/// Encapsulates a change to the style provider for an enum property.
/// </summary>
public class EnumProviderChangeEvent<TObject, TStyle> : StyleProviderChangeEvent<TObject, TStyle> where TStyle : struct, Enum
{
    /// <summary>
    /// Create a new <see cref="EnumProviderChangeEvent{TObject, TStyle}"/> instance.
    /// </summary>
    /// <param name="strategy">The new style variation strategy.</param>
    /// <param name="getValue">Function to get the current value of the style provider.</param>
    /// <param name="setValue">Function to set a new value for the style provider.</param>
    public EnumProviderChangeEvent(
        StyleVariationStrategy strategy,
        Func<TObject, IStyleProvider<TStyle>> getValue,
        Action<TObject, IStyleProvider<TStyle>> setValue) : base(
            strategy,
            getValue,
            setValue)
    {
    }

    /// <inheritdoc />
    public override ICommand ToCommand(TObject model)
    {
        return new EnumStyleProviderChangeCommand<TObject, TStyle>(
            model,
            strategy,
            getValue,
            setValue);
    }
}
