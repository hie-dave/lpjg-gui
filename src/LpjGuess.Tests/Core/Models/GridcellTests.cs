using LpjGuess.Core.Models;

namespace LpjGuess.Tests.Core.Models;

public class GridcellTests
{
    [Fact]
    public void Constructor_ThrowsForInvalidLatitude()
    {
        Assert.Throws<ArgumentException>(() => new Gridcell(-91, 0));
        Assert.Throws<ArgumentException>(() => new Gridcell(91, 0));
    }

    [Fact]
    public void Constructor_ThrowsForInvalidLongitude()
    {
        Assert.Throws<ArgumentException>(() => new Gridcell(0, -181));
        Assert.Throws<ArgumentException>(() => new Gridcell(0, 181));
    }

    [Fact]
    public void Name_ReturnsExplicitOrDerivedName()
    {
        Assert.Equal("SiteA", new Gridcell(10, 20, "SiteA").Name);
        Assert.Equal("(10, 20)", new Gridcell(10, 20).Name);
    }

    [Fact]
    public void Equals_UsesTolerance()
    {
        var a = new Gridcell(10.00, 20.00);
        var b = new Gridcell(10.005, 20.005);
        var c = new Gridcell(10.03, 20.03);

        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
        Assert.False(a.Equals("not-a-gridcell"));
    }
}
