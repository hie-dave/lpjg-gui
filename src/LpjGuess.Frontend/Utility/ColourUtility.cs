using System.Globalization;
using LpjGuess.Core.Models.Graphing;
using OxyPlot;

namespace LpjGuess.Frontend.Utility;

/// <summary>
/// Utility class for color operations.
/// </summary>
public static class ColourUtility
{
    /// <summary>
    /// Default colors for series when no color is specified.
    /// </summary>
    public static readonly string[] DefaultColors = new[]
    {
        "#1f77b4", // Blue
        "#ff7f0e", // Orange
        "#2ca02c", // Green
        "#d62728", // Red
        "#9467bd", // Purple
        "#8c564b", // Brown
        "#e377c2", // Pink
        "#7f7f7f", // Gray
        "#bcbd22", // Olive
        "#17becf"  // Cyan
    };

    /// <summary>
    /// Convert a <see cref="Colour"/> to an <see cref="OxyColor"/>.
    /// </summary>
    /// <param name="colour">The colour to convert.</param>
    /// <returns>The converted colour.</returns>
    public static OxyColor ToOxyColor(this Colour colour)
    {
        return OxyColor.FromArgb(colour.A, colour.R, colour.G, colour.B);
    }

    /// <summary>
    /// Convert a hex color string to an OxyColor.
    /// </summary>
    /// <param name="hexColor">Hex color string (e.g., "#FF0000" or "#FFFF0000").</param>
    /// <returns>OxyColor object.</returns>
    public static OxyColor HexToOxyColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor))
            return OxyColors.Blue;

        try
        {
            hexColor = hexColor.TrimStart('#');

            if (hexColor.Length == 6)
            {
                // RGB format
                int r = int.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber);
                int g = int.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber);
                int b = int.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber);
                return OxyColor.FromRgb((byte)r, (byte)g, (byte)b);
            }
            else if (hexColor.Length == 8)
            {
                // ARGB format
                int a = int.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber);
                int r = int.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber);
                int g = int.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber);
                int b = int.Parse(hexColor.Substring(6, 2), NumberStyles.HexNumber);
                return OxyColor.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
            }

            return OxyColors.Blue;
        }
        catch
        {
            return OxyColors.Blue;
        }
    }

    /// <summary>
    /// Convert an OxyColor to a hex color string.
    /// </summary>
    /// <param name="color">OxyColor object.</param>
    /// <param name="includeAlpha">Whether to include the alpha channel.</param>
    /// <returns>Hex color string.</returns>
    public static string OxyColorToHex(OxyColor color, bool includeAlpha = true)
    {
        if (includeAlpha)
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        else
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// Get a color from the default color palette by index.
    /// </summary>
    /// <param name="index">Index in the color palette.</param>
    /// <returns>Hex color string.</returns>
    public static string GetColorByIndex(int index)
    {
        return DefaultColors[index % DefaultColors.Length];
    }

    /// <summary>
    /// Convert a GDK RGBA to a hex color string.
    /// </summary>
    /// <param name="rgba">The GDK RGBA to convert.</param>
    /// <returns>The hex color string.</returns>
    public static string ToHex(this Gdk.RGBA rgba)
    {
        // Each colour is a float in the range [0, 1]. Convert to bytes.
        byte alpha = (byte)(rgba.Alpha * 255);
        byte red = (byte)(rgba.Red * 255);
        byte green = (byte)(rgba.Green * 255);
        byte blue = (byte)(rgba.Blue * 255);

        return $"#{alpha:X2}{red:X2}{green:X2}{blue:X2}";
    }

    /// <summary>
    /// Convert a <see cref="Colour"/> to a GDK RGBA.
    /// </summary>
    /// <param name="colour">The colour to convert.</param>
    /// <returns>The GDK RGBA.</returns>
    public static Gdk.RGBA ToRgba(this Colour colour)
    {
        Gdk.RGBA rgba = new Gdk.RGBA();
        rgba.Alpha = colour.A / 255f;
        rgba.Red = colour.R / 255f;
        rgba.Green = colour.G / 255f;
        rgba.Blue = colour.B / 255f;
        return rgba;
    }

    /// <summary>
    /// Convert a GDK RGBA to a <see cref="Colour"/>.
    /// </summary>
    /// <param name="rgba">The GDK RGBA to convert.</param>
    /// <returns>The <see cref="Colour"/>.</returns>
    public static Colour ToColour(this Gdk.RGBA rgba)
    {
        byte red = (byte)(rgba.Red * 255);
        byte green = (byte)(rgba.Green * 255);
        byte blue = (byte)(rgba.Blue * 255);
        byte alpha = (byte)(rgba.Alpha * 255);
        return new Colour(red, green, blue, alpha);
    }

    /// <summary>
    /// Convert a hex color string to a GDK RGBA.
    /// </summary>
    /// <param name="colour">The hex color string.</param>
    /// <returns>The GDK RGBA.</returns>
    public static Gdk.RGBA FromString(string colour)
    {
        Gdk.RGBA rgba = new Gdk.RGBA();
        if (!rgba.Parse(colour))
        {
            // TODO: emit a warning?
            Console.Error.WriteLine($"WARNING: failed to parse Gdk.RGBA from colour string: '{colour}'");
            rgba.Parse("black");
        }
        return rgba;
    }
}
