using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Events;

/// <summary>
/// Encapsulates a change to a model's colour provider.
/// </summary>
public class ColourProviderChangeEvent<TObject> : StyleProviderChangeEvent<TObject, Colour> where TObject : notnull
{
    /// <summary>
    /// Create a new <see cref="ColourProviderChangeEvent{TObject}"/> instance.
    /// </summary>
    /// <param name="strategy">The new style variation strategy.</param>
    /// <param name="getValue">Function to get the current value of the style provider.</param>
    /// <param name="setValue">Function to set a new value for the style provider.</param>
    public ColourProviderChangeEvent(
        StyleVariationStrategy strategy,
        Func<TObject, IStyleProvider<Colour>> getValue,
        Action<TObject, IStyleProvider<Colour>> setValue) : base(
            strategy,
            getValue,
            setValue)
    {
    }

    /// <inheritdoc />
    public override ICommand ToCommand(TObject model)
    {
        return new ColourStyleProviderChangeCommand<TObject>(
            model,
            strategy,
            getValue,
            setValue);
    }
}
