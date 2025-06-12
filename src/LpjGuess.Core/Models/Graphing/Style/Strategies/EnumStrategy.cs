namespace LpjGuess.Core.Models.Graphing.Style.Strategies;

/// <summary>
/// A style value strategy which provides instances of a particular enum type.
/// </summary>
/// <typeparam name="T"></typeparam>
public class EnumStrategy<T> : StyleStrategyBase<T> where T : struct, Enum
{
    /// <summary>
    /// Create a new <see cref="EnumStrategy{T}"/> instance.
    /// </summary>
    public EnumStrategy() : base(Enum.GetValues<T>()) { }

    /// <inheritdoc />
    public override void Initialise(int totalStylesCount) { }
}
