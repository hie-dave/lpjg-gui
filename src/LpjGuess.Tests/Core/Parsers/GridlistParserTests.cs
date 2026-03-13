using LpjGuess.Core.Models;
using LpjGuess.Core.Parsers;
using Microsoft.Extensions.Logging;
using Moq;

namespace LpjGuess.Tests.Core.Parsers;

public class GridlistParserTests : IDisposable
{
    private readonly Mock<ILogger<GridlistParser>> logger;
    private readonly GridlistParser parser;
    private readonly TempDirectory testDir;

    public GridlistParserTests()
    {
        testDir = TempDirectory.Create("gridlist_parser_tests");
        logger = new Mock<ILogger<GridlistParser>>();
        parser = new GridlistParser(logger.Object);
    }

    public void Dispose()
    {
        testDir.Dispose();
    }

    [Fact]
    public async Task ParseAsync_ParsesGridcells_WithOptionalName()
    {
        string path = await CreateGridlistFile("""
            10.5 11.5 Site A
            20 30
            """);

        Gridcell[] result = (await parser.ParseAsync(path)).ToArray();

        Assert.Equal(2, result.Length);
        Assert.Equal(11.5, result[0].Latitude);
        Assert.Equal(10.5, result[0].Longitude);
        Assert.Equal("Site A", result[0].Name);
        Assert.Equal(30, result[1].Latitude);
        Assert.Equal(20, result[1].Longitude);
        Assert.Equal("(30, 20)", result[1].Name);
    }

    [Fact]
    public async Task ParseAsync_IgnoresBlankLines_AndHandlesMixedWhitespace()
    {
        string path = await CreateGridlistFile("""
            10.5	11.5 SiteA

            -120.25     -45.5     Site B
            
            """);

        Gridcell[] result = (await parser.ParseAsync(path)).ToArray();

        Assert.Equal(2, result.Length);
        Assert.Equal("SiteA", result[0].Name);
        Assert.Equal("Site B", result[1].Name);
    }

    [Fact]
    public async Task ParseAsync_Throws_ForLineWithTooFewColumns()
    {
        string path = await CreateGridlistFile("123");

        InvalidDataException error = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseAsync(path));

        Assert.Contains("line 1", error.Message);
        Assert.Contains("at least 2 parts", error.Message);
    }

    [Fact]
    public async Task ParseAsync_Throws_ForInvalidLongitude()
    {
        string path = await CreateGridlistFile("abc 20 SiteA");

        InvalidDataException error = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseAsync(path));

        Assert.Contains("failed to parse longitude", error.Message);
    }

    [Fact]
    public async Task ParseAsync_Throws_ForInvalidLatitude()
    {
        string path = await CreateGridlistFile("10.5 xyz SiteA");

        InvalidDataException error = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseAsync(path));

        Assert.Contains("failed to parse latitude", error.Message);
    }

    [Theory]
    [InlineData("181 20 SiteA")]
    [InlineData("-181 20 SiteA")]
    public async Task ParseAsync_Throws_ForLongitudeOutOfRange(string content)
    {
        string path = await CreateGridlistFile(content);

        InvalidDataException error = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseAsync(path));

        Assert.Contains("longitude must be in range [-180, 180]", error.Message);
    }

    [Theory]
    [InlineData("10 91 SiteA")]
    [InlineData("10 -91 SiteA")]
    public async Task ParseAsync_Throws_ForLatitudeOutOfRange(string content)
    {
        string path = await CreateGridlistFile(content);

        InvalidDataException error = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseAsync(path));

        Assert.Contains("latitude must be in range [-90, 90]", error.Message);
    }

    private async Task<string> CreateGridlistFile(string content)
    {
        string fileName = $"gridlist-{Guid.NewGuid()}.txt";
        string path = Path.Combine(testDir.AbsolutePath, fileName);
        await File.WriteAllTextAsync(path, content);
        return path;
    }
}
