using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Core.Models.Graphing.Style.Providers;
using LpjGuess.Frontend.Data;
using LpjGuess.Frontend.Utility;
using Microsoft.Extensions.Logging;
using Moq;
using OxyPlot;

using DomainScatterSeries = LpjGuess.Core.Models.Graphing.Series.ScatterSeries;
using OxyScatterSeries = OxyPlot.Series.ScatterSeries;

namespace LpjGuess.Tests.Frontend.Utility;

public class OxyPlotConverterTests
{
    [Fact]
    public async Task ToOxySeriesAsync_ScatterSeries_RendersUnconnectedPoints()
    {
        var source = new ModelOutput("file_lai", "Height", ["Total"], []);
        var series = new DomainScatterSeries(
            string.Empty,
            new FixedStyleProvider<Colour>(Colours.Red),
            source,
            AxisPosition.Bottom,
            AxisPosition.Left);
        var context = new SeriesContext(
            "experiment",
            "simulation",
            new Gridcell(-33.1, 151.2, "site"),
            "Total");
        var data = new SeriesData(
            "Generated title",
            context,
            [new DataPoint(1, 2), new DataPoint(3, 4)]);

        var dataProviderFactory = new Mock<IDataProviderFactory>();
        dataProviderFactory
            .Setup(f => f.ReadAsync(source, It.IsAny<CancellationToken>()))
            .ReturnsAsync([data]);
        var logger = new Mock<ILogger<OxyPlotConverter>>();
        var converter = new OxyPlotConverter(dataProviderFactory.Object, logger.Object);

        OxyScatterSeries oxySeries = Assert.IsType<OxyScatterSeries>(
            Assert.Single(await converter.ToOxySeriesAsync(
                series,
                new StyleContext(1),
                CancellationToken.None)));

        Assert.Equal("Generated title", oxySeries.Title);
        Assert.Equal(MarkerType.Circle, oxySeries.MarkerType);
        Assert.Equal(OxyColors.Red, oxySeries.MarkerFill);
        Assert.Collection(
            oxySeries.Points,
            point =>
            {
                Assert.Equal(1, point.X);
                Assert.Equal(2, point.Y);
            },
            point =>
            {
                Assert.Equal(3, point.X);
                Assert.Equal(4, point.Y);
            });
    }

    [Fact]
    public async Task ToOxySeriesAsync_ScatterSeries_UsesExplicitTitle()
    {
        var source = new ModelOutput("file_lai", "Height", ["Total"], []);
        var series = new DomainScatterSeries(
            "Observed values",
            new FixedStyleProvider<Colour>(Colours.Blue),
            source,
            AxisPosition.Bottom,
            AxisPosition.Left);
        var data = new SeriesData(
            "Generated title",
            new SeriesContext("experiment", "simulation", new Gridcell(0, 0), "Total"),
            []);

        var dataProviderFactory = new Mock<IDataProviderFactory>();
        dataProviderFactory
            .Setup(f => f.ReadAsync(source, It.IsAny<CancellationToken>()))
            .ReturnsAsync([data]);
        var converter = new OxyPlotConverter(
            dataProviderFactory.Object,
            Mock.Of<ILogger<OxyPlotConverter>>());

        OxyScatterSeries oxySeries = Assert.IsType<OxyScatterSeries>(
            Assert.Single(await converter.ToOxySeriesAsync(
                series,
                new StyleContext(1),
                CancellationToken.None)));

        Assert.Equal("Observed values", oxySeries.Title);
    }
}
