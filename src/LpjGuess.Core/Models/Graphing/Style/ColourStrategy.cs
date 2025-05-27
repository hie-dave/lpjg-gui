namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A strategy for selecting colours based on an index.
/// </summary>
public class ColourStrategy : StyleStrategyBase<Colour>
{
    /// <summary>
    /// Create a new <see cref="ColourStrategy"/> instance.
    /// </summary>
    public ColourStrategy() : this(Colours.Palette) { }

    /// <summary>
    /// Create a new <see cref="ColourStrategy"/> instance.
    /// </summary>
    /// <param name="palette">The palette of colours to use.</param>
    public ColourStrategy(IEnumerable<Colour> palette) : base(palette) { }
}
