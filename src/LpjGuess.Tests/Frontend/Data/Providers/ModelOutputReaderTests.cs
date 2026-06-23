using LpjGuess.Core.Models;
using LpjGuess.Frontend.Data.Providers;

namespace LpjGuess.Tests.Frontend.Data.Providers;

public class ModelOutputReaderTests
{
    private static SeriesContext BuildContext(
        string layer,
        int? stand = 1,
        int? patch = 2,
        int? individual = 3)
    {
        return new SeriesContext(
            "experiment",
            "simulation",
            new Gridcell(-33.1, 151.2, "site"),
            layer,
            stand,
            patch,
            individual,
            "TrBE");
    }

    [Fact]
    public void AreSameSeries_MatchesDifferentXAxisAndYAxisLayers()
    {
        SeriesContext xContext = BuildContext("Height");
        SeriesContext yContext = BuildContext("Biomass");

        Assert.NotEqual(xContext, yContext);
        Assert.True(ModelOutputReader.AreSameSeries(xContext, yContext));
    }

    [Theory]
    [InlineData(2, 2, 3)]
    [InlineData(1, 4, 3)]
    [InlineData(1, 2, 5)]
    public void AreSameSeries_DoesNotMatchDifferentSeriesDimensions(
        int stand,
        int patch,
        int individual)
    {
        SeriesContext xContext = BuildContext("Height");
        SeriesContext yContext = BuildContext("Biomass", stand, patch, individual);

        Assert.False(ModelOutputReader.AreSameSeries(xContext, yContext));
    }
}
