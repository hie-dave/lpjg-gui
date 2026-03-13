using LpjGuess.Core.Parsers;

namespace LpjGuess.Tests.Core.Parsers;

public class GridlistParserTests
{
    [Fact]
    public async Task Constructor_ParsesGridcellsAndIgnoresBlankLines()
    {
        using TempDirectory temp = TempDirectory.Create();
        string path = Path.Combine(temp.AbsolutePath, "gridlist.txt");

        await File.WriteAllTextAsync(path, """
            10.5 11.5 SiteA

            20 30
            """);

        var parser = new GridlistParser(path);

        Assert.Equal(2, parser.Gridcells.Count);
        Assert.Equal("SiteA", parser.GetName(10.5, 11.5));
        Assert.Equal("(30, 20)", parser.GetName(20, 30));
    }

    [Fact]
    public async Task GetName_Throws_WhenCoordinateMissing()
    {
        using TempDirectory temp = TempDirectory.Create();
        string path = Path.Combine(temp.AbsolutePath, "gridlist.txt");
        await File.WriteAllTextAsync(path, "10 20 SiteA");

        var parser = new GridlistParser(path);

        Assert.Throws<ArgumentException>(() => parser.GetName(11, 22));
    }

    [Fact]
    public async Task Constructor_Throws_ForInvalidLineFormat()
    {
        using TempDirectory temp = TempDirectory.Create();
        string path = Path.Combine(temp.AbsolutePath, "gridlist.txt");
        await File.WriteAllTextAsync(path, "123");

        var error = Assert.ThrowsAny<Exception>(() => new GridlistParser(path));

        Assert.Contains("Invalid gridlist line", error.ToString());
    }
}
