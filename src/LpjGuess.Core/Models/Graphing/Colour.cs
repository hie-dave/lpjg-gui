using Newtonsoft.Json;

namespace LpjGuess.Core.Models.Graphing;

/// <summary>
/// A 32-bit colour, stored as ARGB.
/// </summary>
[Serializable]
public struct Colour
{
    /// <summary>
    /// The alpha component of the colour.
    /// </summary>
    public byte A { get; private init; }

    /// <summary>
    /// The red component of the colour.
    /// </summary>
    public byte R { get; private init; }

    /// <summary>
    /// The green component of the colour.
    /// </summary>
    public byte G { get; private init; }

    /// <summary>
    /// The blue component of the colour.
    /// </summary>
    public byte B { get; private init; }

    /// <summary>
    /// Create a new <see cref="Colour"/> instance.
    /// </summary>
    /// <param name="r">The red component of the colour.</param>
    /// <param name="g">The green component of the colour.</param>
    /// <param name="b">The blue component of the colour.</param>
    /// <param name="a">The alpha component of the colour.</param>
    [JsonConstructor]
    public Colour(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>
    /// Create a new <see cref="Colour"/> instance with full opacity.
    /// </summary>
    /// <param name="r">The red component of the colour.</param>
    /// <param name="g">The green component of the colour.</param>
    /// <param name="b">The blue component of the colour.</param>
    public Colour(byte r, byte g, byte b) : this(r, g, b, 255)
    {
    }

    /// <summary>
    /// Parse from hex string.
    /// </summary>
    /// <param name="hex">The hex string to parse.</param>
    public static Colour FromHex(string hex)
    {
        // Handle various formats: #RGB, #RGBA, #RRGGBB, #RRGGBBAA
        hex = hex.TrimStart('#');

        if (hex.Length == 3 || hex.Length == 4)
        {
            // Convert #RGB to #RRGGBB or #RGBA to #RRGGBBAA
            string expanded = string.Concat(hex.Select(c => new string(c, 2)));
            hex = expanded;
        }

        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        byte a = hex.Length >= 8 ? Convert.ToByte(hex.Substring(6, 2), 16) : (byte)255;

        return new Colour(r, g, b, a);
    }

    /// <summary>
    /// Create a new <see cref="Colour"/> from HSV (Hue, Saturation, Value) components.
    /// </summary>
    /// <param name="hue">The hue in degrees (0-360).</param>
    /// <param name="saturation">The saturation (0-1).</param>
    /// <param name="value">The value/brightness (0-1).</param>
    /// <returns>A new <see cref="Colour"/> instance.</returns>
    public static Colour FromHsv(double hue, double saturation, double value)
    {
        // Ensure inputs are in valid ranges
        hue = ((hue % 360) + 360) % 360; // Normalize to [0, 360)
        saturation = Math.Clamp(saturation, 0, 1);
        value = Math.Clamp(value, 0, 1);

        double chroma = value * saturation;
        double huePrime = hue / 60.0;
        double x = chroma * (1 - Math.Abs((huePrime % 2) - 1));
        double m = value - chroma;

        double r, g, b;
        if (huePrime <= 1)
        {
            r = chroma;
            g = x;
            b = 0;
        }
        else if (huePrime <= 2)
        {
            r = x;
            g = chroma;
            b = 0;
        }
        else if (huePrime <= 3)
        {
            r = 0;
            g = chroma;
            b = x;
        }
        else if (huePrime <= 4)
        {
            r = 0;
            g = x;
            b = chroma;
        }
        else if (huePrime <= 5)
        {
            r = x;
            g = 0;
            b = chroma;
        }
        else
        {
            r = chroma;
            g = 0;
            b = x;
        }

        // Convert to bytes in [0, 255] range
        byte rByte = (byte)Math.Round((r + m) * 255);
        byte gByte = (byte)Math.Round((g + m) * 255);
        byte bByte = (byte)Math.Round((b + m) * 255);

        return new Colour(rByte, gByte, bByte);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"#{R:X2}{G:X2}{B:X2}{(A < 255 ? A.ToString("X2") : "")}";
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not Colour other)
            return false;

        return A == other.A && R == other.R && G == other.G && B == other.B;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(A, R, G, B);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Colour left, Colour right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Colour left, Colour right) => !left.Equals(right);
}
