namespace LpjGuess.Runner.Extensions;

/// <summary>
/// Provides extension methods for <see cref="byte"/> arrays.
/// </summary>
public static class ByteArrayExtensions
{
    /// <summary>
    /// Converts the specified byte array to a hex string.
    /// </summary>
    /// <param name="data">The byte array.</param>
    /// <returns>The hex string.</returns>
    public static string ToHex(this byte[] data)
    {
        char[] c = new char[data.Length * 2];
        int b;
        for (int i = 0; i < data.Length; i++)
        {
            b = data[i] >> 4;
            c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
            b = data[i] & 0xF;
            c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
        }
        return new string(c).ToLowerInvariant();
    }
}
