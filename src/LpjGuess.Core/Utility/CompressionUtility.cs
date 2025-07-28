using System.IO.Compression;
using System.Text;

namespace LpjGuess.Core.Utility;

/// <summary>
/// Utility methods for compressing and decompressing text.
/// </summary>
public static class CompressionUtility
{
    /// <summary>
    /// Compresses a string using GZip compression.
    /// </summary>
    /// <param name="text">The string to compress.</param>
    /// <returns>The compressed byte array.</returns>
    public static byte[] CompressText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<byte>();

        var bytes = Encoding.UTF8.GetBytes(text);
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
            msi.CopyTo(gs);

        return mso.ToArray();
    }

    /// <summary>
    /// Compresses a string using GZip compression asynchronously.
    /// </summary>
    /// <param name="text">The string to compress.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The compressed byte array.</returns>
    public static async Task<byte[]> CompressTextAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<byte>();

        var bytes = Encoding.UTF8.GetBytes(text);
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
            await msi.CopyToAsync(gs, ct);

        return mso.ToArray();
    }

    /// <summary>
    /// Decompresses a byte array using GZip decompression.
    /// </summary>
    /// <param name="compressed">The byte array to decompress.</param>
    /// <returns>The decompressed string.</returns>
    public static string DecompressToText(byte[] compressed)
    {
        if (compressed == null || compressed.Length == 0)
            return string.Empty;

        using var msi = new MemoryStream(compressed);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            gs.CopyTo(mso);

        return Encoding.UTF8.GetString(mso.ToArray());
    }

    /// <summary>
    /// Decompresses a byte array using GZip decompression.
    /// </summary>
    /// <param name="compressed">The byte array to decompress.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The decompressed string.</returns>
    public static async Task<string> DecompressToTextAsync(byte[] compressed, CancellationToken ct = default)
    {
        if (compressed == null || compressed.Length == 0)
            return string.Empty;

        using var msi = new MemoryStream(compressed);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            await gs.CopyToAsync(mso, ct);

        return Encoding.UTF8.GetString(mso.ToArray());
    }
}
