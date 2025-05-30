namespace LpjGuess.Core.Models.Graphing;

/// <summary>
/// A legend for a graph.
/// </summary>
public class Legend
{
    /// <inheritdoc/>
    public LegendPosition Position { get; set; }

    /// <inheritdoc/>
    public LegendPlacement Placement { get; set; }

    /// <inheritdoc/>
    public LegendOrientation Orientation { get; set; }

    /// <inheritdoc/>
    public Colour BackgroundColour { get; set; }

    /// <inheritdoc/>
    public Colour BorderColour { get; set; }

    /// <summary>
    /// Create a new <see cref="Legend"/> instance.
    /// </summary>
    /// <param name="position">The position of the legend.</param>
    /// <param name="placement">The placement of the legend.</param>
    /// <param name="orientation">The orientation of the legend.</param>
    /// <param name="backgroundColour">The background colour of the legend.</param>
    /// <param name="borderColour">The border colour of the legend.</param>
    public Legend(
        LegendPosition position,
        LegendPlacement placement,
        LegendOrientation orientation,
        Colour backgroundColour,
        Colour borderColour)
    {
        Position = position;
        Placement = placement;
        Orientation = orientation;
        BackgroundColour = backgroundColour;
        BorderColour = borderColour;
    }

    /// <summary>
    /// Default constructor provided for serialization purposes.
    /// </summary>
    public Legend() : this(
        LegendPosition.TopLeft,
        LegendPlacement.Inside,
        LegendOrientation.Vertical,
        Colours.Transparent,
        Colours.Transparent)
    {
    }
}
