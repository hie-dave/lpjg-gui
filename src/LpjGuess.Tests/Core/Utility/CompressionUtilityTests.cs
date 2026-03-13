using LpjGuess.Core.Utility;

namespace LpjGuess.Tests.Core.Utility;

public class CompressionUtilityTests
{
    [Fact]
    public void CompressAndDecompressText_RoundTrips()
    {
        const string text = "hello world — 你好 — 123";

        byte[] compressed = CompressionUtility.CompressText(text);
        string decompressed = CompressionUtility.DecompressToText(compressed);

        Assert.NotEmpty(compressed);
        Assert.Equal(text, decompressed);
    }

    [Fact]
    public async Task CompressAndDecompressTextAsync_RoundTrips()
    {
        const string text = "async payload";

        byte[] compressed = await CompressionUtility.CompressTextAsync(text);
        string decompressed = await CompressionUtility.DecompressToTextAsync(compressed);

        Assert.NotEmpty(compressed);
        Assert.Equal(text, decompressed);
    }

    [Fact]
    public async Task EmptyOrNullInputs_ReturnExpectedDefaults()
    {
        Assert.Empty(CompressionUtility.CompressText(string.Empty));
        Assert.Equal(string.Empty, CompressionUtility.DecompressToText(Array.Empty<byte>()));
        Assert.Equal(string.Empty, CompressionUtility.DecompressToText(null!));

        Assert.Empty(await CompressionUtility.CompressTextAsync(string.Empty));
        Assert.Equal(string.Empty, await CompressionUtility.DecompressToTextAsync(Array.Empty<byte>()));
        Assert.Equal(string.Empty, await CompressionUtility.DecompressToTextAsync(null!));
    }
}
