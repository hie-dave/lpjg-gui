using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Utility;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// A command to change the style provider of a model's style property.
/// </summary>
public class ColourStyleProviderChangeCommand<TObject> : StyleProviderChangeCommand<TObject, Colour>
{
    /// <summary>
    /// Create a new <see cref="ColourStyleProviderChangeCommand{TObject}"/>
    /// instance.
    /// </summary>
    public ColourStyleProviderChangeCommand(
        TObject target,
        StyleVariationStrategy strategy,
        Func<TObject, IStyleProvider<Colour>> getValue,
        Action<TObject, IStyleProvider<Colour>> setValue) : base(
            target,
            strategy,
            getValue,
            setValue)
    {
    }

    /// <inheritdoc />
    protected override IStyleStrategy<Colour> CreateStrategy()
    {
        return new ColourStrategy();
    }
}
