namespace LpjGuess.Core.Models.Graphing;

/// <summary>
/// A collection of common colours.
/// </summary>
public static class Colours
{
    /// <summary>
    /// Black.
    /// </summary>
    public static readonly Colour Black = new Colour(0, 0, 0);

    /// <summary>
    /// White.
    /// </summary>
    public static readonly Colour White = new Colour(255, 255, 255);

    /// <summary>
    /// Red.
    /// </summary>
    public static readonly Colour Red = new Colour(255, 0, 0);

    /// <summary>
    /// Green.
    /// </summary>
    public static readonly Colour Green = new Colour(0, 255, 0);

    /// <summary>
    /// Blue.
    /// </summary>
    public static readonly Colour Blue = new Colour(0, 0, 255);

    /// <summary>
    /// Yellow.
    /// </summary>
    public static readonly Colour Yellow = new Colour(255, 255, 0);

    /// <summary>
    /// Colour palette optimised for people with various kinds of
    /// colour-blindness.
    /// Wong, B. (2011) Color blindness, Nature Methods, Vol 8, No. 6.
    /// </summary>
    public static readonly IReadOnlyList<Colour> Palette = [
        Colour.FromHex("#e69f00"),
        Colour.FromHex("#56b4e9"),
        Colour.FromHex("#cc79a7"),
        Colour.FromHex("#009e73"),
        Colour.FromHex("#0072b2"),
        Colour.FromHex("#d55e00"),
        Colour.FromHex("#f0e442"),
        Colour.FromHex("#000000")
    ];
}
