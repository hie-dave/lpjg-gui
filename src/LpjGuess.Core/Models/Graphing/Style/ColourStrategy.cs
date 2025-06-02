using LpjGuess.Core.Interfaces.Graphing;

namespace LpjGuess.Core.Models.Graphing.Style;

/// <summary>
/// A strategy for selecting colours based on an index.
/// </summary>
public class ColourStrategy : StyleStrategyBase<Colour>
{
    /// <summary>
    /// The fallback palette to use if the finite palette is not large enough.
    /// </summary>
    private readonly IColourPalette fallbackPalette;

    /// <summary>
    /// The total number of styles required by subsequent calls to GetValue().
    /// </summary>
    private int totalStylesCount;

    /// <summary>
    /// Create a new <see cref="ColourStrategy"/> instance.
    /// </summary>
    public ColourStrategy() : this(Colours.Palette) { }

    /// <summary>
    /// Create a new <see cref="ColourStrategy"/> instance.
    /// </summary>
    /// <param name="palette">The palette of colours to use.</param>
    /// <param name="fallbackPalette">The fallback palette to use if the finite palette is not large enough.</param>
    public ColourStrategy(
        IEnumerable<Colour> palette,
        IColourPalette? fallbackPalette = null) : base(palette)
    {
        this.fallbackPalette = fallbackPalette ?? new RainbowPalette();
    }

    /// <inheritdoc />
    public override Colour GetValue(uint index)
    {
        if (totalStylesCount <= values.Count)
            // Use finite colour palette where feasible.
            return base.GetValue(index);
        else
        {
            // Attempt to generate N evenly-spaced colours from the palette.
            double position = 1.0 * index / totalStylesCount;
            return fallbackPalette.GetColour(position);
        }
    }

    /// <inheritdoc />
    public override void Initialise(int totalStylesCount)
    {
        this.totalStylesCount = totalStylesCount;
    }
}
