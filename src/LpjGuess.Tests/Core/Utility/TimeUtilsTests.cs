using LpjGuess.Core.Utility;

namespace LpjGuess.Tests.Core.Utility;

public class TimeUtilsTests
{
    [Theory]
    [InlineData(59, "59.0 seconds")]
    [InlineData(60, "1.0 minutes")]
    [InlineData(3600, "1.0 hours")]
    [InlineData(86400, "1.0 days")]
    [InlineData(2592000, "1.0 months")]
    [InlineData(31536000, "1.0 years")]
    public void FormatTimeSpan_UsesExpectedUnit(double seconds, string expected)
    {
        string result = TimeUtils.FormatTimeSpan(TimeSpan.FromSeconds(seconds));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Jan", 1)]
    [InlineData("Feb", 2)]
    [InlineData("Mar", 3)]
    [InlineData("Apr", 4)]
    [InlineData("May", 5)]
    [InlineData("Jun", 6)]
    [InlineData("Jul", 7)]
    [InlineData("Aug", 8)]
    [InlineData("Sep", 9)]
    [InlineData("Oct", 10)]
    [InlineData("Nov", 11)]
    [InlineData("Dec", 12)]
    public void GetMonth_ReturnsExpectedMonthNumber(string month, int expected)
    {
        Assert.Equal(expected, TimeUtils.GetMonth(month));
    }

    [Fact]
    public void GetMonth_Throws_ForUnknownMonth()
    {
        Assert.Throws<ArgumentException>(() => TimeUtils.GetMonth("january"));
    }
}
