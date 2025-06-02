namespace LpjGuess.Core.Models.Graphing;

/// <summary>
/// A legend for a graph.
/// </summary>
public class Legend
{
    /// <summary>
    /// Whether the legend is visible.
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>
    /// The position of the legend.
    /// </summary>
    public LegendPosition Position { get; set; }

    /// <summary>
    /// The placement of the legend.
    /// </summary>
    public LegendPlacement Placement { get; set; }

    /// <summary>
    /// The orientation of the legend.
    /// </summary>
    public LegendOrientation Orientation { get; set; }

    /// <summary>
    /// The background colour of the legend.
    /// </summary>
    public Colour BackgroundColour { get; set; }

    /// <summary>
    /// The border colour of the legend.
    /// </summary>
    public Colour BorderColour { get; set; }

    /// <summary>
    /// Create a new <see cref="Legend"/> instance.
    /// </summary>
    /// <param name="visible">Whether the legend is visible.</param>
    /// <param name="position">The position of the legend.</param>
    /// <param name="placement">The placement of the legend.</param>
    /// <param name="orientation">The orientation of the legend.</param>
    /// <param name="backgroundColour">The background colour of the legend.</param>
    /// <param name="borderColour">The border colour of the legend.</param>
    public Legend(
        bool visible,
        LegendPosition position,
        LegendPlacement placement,
        LegendOrientation orientation,
        Colour backgroundColour,
        Colour borderColour)
    {
        Visible = visible;
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
        true,
        LegendPosition.TopLeft,
        LegendPlacement.Inside,
        LegendOrientation.Vertical,
        Colours.Transparent,
        Colours.Transparent)
    {
    }
}
