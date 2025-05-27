using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Utility;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// A command to change the style provider of a model's style property.
/// </summary>
public class EnumStyleProviderChangeCommand<TObject, TStyle> : StyleProviderChangeCommand<TObject, TStyle> where TStyle : struct, Enum
{
    /// <summary>
    /// Create a new <see cref="EnumStyleProviderChangeCommand{TObject, TStyle}"/> instance.
    /// </summary>
    public EnumStyleProviderChangeCommand(
        TObject target,
        StyleVariationStrategy strategy,
        Func<TObject, IStyleProvider<TStyle>> getValue,
        Action<TObject, IStyleProvider<TStyle>> setValue) : base(
            target,
            strategy,
            getValue,
            setValue)
    {
    }

    /// <inheritdoc />
    protected override IStyleStrategy<TStyle> CreateStrategy()
    {
        return new EnumStrategy<TStyle>();
    }
}
