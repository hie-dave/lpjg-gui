using LpjGuess.Core.Models;
using LpjGuess.Core.Parsers;
using Microsoft.Extensions.Logging;
using Moq;

namespace LpjGuess.Tests.Core.Parsers;

public class GridlistTests
{
    private readonly Mock<ILogger<GridlistParser>> logger = new();
    private readonly GridlistParser parser;

    public GridlistTests()
    {
        logger = new Mock<ILogger<GridlistParser>>();
        parser = new GridlistParser(logger.Object);
    }

    [Fact]
    public async Task Constructor_ParsesGridcellsAndIgnoresBlankLines()
    {
        using TempDirectory temp = TempDirectory.Create();
        string path = Path.Combine(temp.AbsolutePath, "gridlist.txt");

        await File.WriteAllTextAsync(path, """
            10.5 11.5 SiteA

            20 30
            """);

        Gridlist list = new Gridlist(path, parser);

        Assert.Equal(2, list.Gridcells.Count);
        Assert.Equal("SiteA", list.GetName(10.5, 11.5));
        Assert.Equal("(30, 20)", list.GetName(20, 30));
    }

    [Fact]
    public async Task GetName_Throws_WhenCoordinateMissing()
    {
        using TempDirectory temp = TempDirectory.Create();
        string path = Path.Combine(temp.AbsolutePath, "gridlist.txt");
        await File.WriteAllTextAsync(path, "10 20 SiteA");

        var parser = new Gridlist(path, this.parser);

        Assert.Throws<ArgumentException>(() => parser.GetName(11, 22));
    }

    [Fact]
    public async Task Constructor_Throws_ForInvalidLineFormat()
    {
        using TempDirectory temp = TempDirectory.Create();
        string path = Path.Combine(temp.AbsolutePath, "gridlist.txt");
        await File.WriteAllTextAsync(path, "123");

        var error = Assert.ThrowsAny<Exception>(() => new Gridlist(path, parser));

        Assert.Contains("must contain at least 2", error.ToString());
    }
}
