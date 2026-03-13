using LpjGuess.Core.Logging;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Tests.Core.Logging;

public class LogFilterTests
{
    [Fact]
    public void Parse_ParsesCategoryAndLevel()
    {
        LogFilter filter = LogFilter.Parse("My.Category=Warning");

        Assert.Equal("My.Category", filter.Category);
        Assert.Equal(LogLevel.Warning, filter.Level);
    }

    [Fact]
    public void Parse_ThrowsWhenFormatInvalid()
    {
        Assert.Throws<ArgumentException>(() => LogFilter.Parse("NoEqualsSign"));
    }
}
