using LpjGuess.Core.Interfaces.Graphing;

namespace LpjGuess.Core.Models.Graphing;

/// <summary>
/// A colour palette which generates colours by varying hue around the colour
/// wheel while maintaining constant saturation and value. This produces visually
/// distinct colours that are suitable for data visualization.
/// </summary>
public class RainbowPalette : IColourPalette
{
    /// <summary>
    /// The saturation to use for all colours (between 0 and 1).
    /// </summary>
    private readonly double saturation;

    /// <summary>
    /// The value/brightness to use for all colours (between 0 and 1).
    /// </summary>
    private readonly double value;

    /// <summary>
    /// Create a new <see cref="RainbowPalette"/> instance.
    /// </summary>
    public RainbowPalette() : this(0.85, 0.9) { }

    /// <summary>
    /// Create a new <see cref="RainbowPalette"/> instance.
    /// </summary>
    /// <param name="saturation">The saturation to use for all colours.</param>
    /// <param name="value">The value/brightness to use for all colours.</param>
    public RainbowPalette(double saturation, double value)
    {
        this.saturation = saturation;
        this.value = value;
    }

    /// <inheritdoc />
    public Colour GetColour(double position)
    {
        // Map position to hue (0-360 degrees).
        // We multiply by 360 to convert from [0,1] to degrees, then offset by
        // 240 to start from blue (since red can be harsh as a starting color).
        double hue = ((position * 360.0) + 240.0) % 360.0;
        return Colour.FromHsv(hue, saturation, value);
    }
}
