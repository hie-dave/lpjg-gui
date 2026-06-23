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
    public void MergeDataSources_MatchesContextAndJoinCoordinate()
    {
        SeriesContext xContext = BuildContext("Observed");
        SeriesContext yContext = BuildContext("Predicted");
        var xData = new SeriesData(
            "Observations",
            xContext,
            [new DataPoint(50, 10), new DataPoint(60, 20)],
            [1, 2]);
        var yData = new SeriesData(
            "Predictions",
            yContext,
            [new DataPoint(80, 200), new DataPoint(70, 100), new DataPoint(90, 300)],
            [2, 1, 3]);

        SeriesData merged = Assert.Single(
            OxyPlotConverter.MergeDataSources([xData], [yData]));

        Assert.Equal("Predictions vs Observations", merged.Name);
        Assert.Equal(yContext, merged.Context);
        Assert.Collection(
            merged.Data,
            point =>
            {
                Assert.Equal(10, point.X);
                Assert.Equal(100, point.Y);
            },
            point =>
            {
                Assert.Equal(20, point.X);
                Assert.Equal(200, point.Y);
            });
    }

    [Fact]
    public void MergeDataSources_DoesNotMatchDifferentSeriesContext()
    {
        var xData = new SeriesData("X", BuildContext("X"), [new DataPoint(1, 10)]);
        var yData = new SeriesData(
            "Y",
            BuildContext("Y", patch: 99),
            [new DataPoint(1, 100)]);

        Assert.Empty(OxyPlotConverter.MergeDataSources([xData], [yData]));
    }

    [Fact]
    public async Task ToOxySeriesAsync_IndependentSources_PlotsSourceValues()
    {
        var xSource = new ModelOutput("file_lai", "Date", ["Observed"], []);
        var ySource = new ModelOutput("file_anpp", "Date", ["Predicted"], []);
        var series = new DomainScatterSeries(
            "Comparison",
            new FixedStyleProvider<Colour>(Colours.Red),
            xSource,
            ySource,
            AxisPosition.Bottom,
            AxisPosition.Left);
        var xData = new SeriesData(
            "Observed",
            BuildContext("Observed"),
            [new DataPoint(1, 10)]);
        var yData = new SeriesData(
            "Predicted",
            BuildContext("Predicted"),
            [new DataPoint(1, 100)]);
        var unusedXData = new SeriesData(
            "Unused X",
            BuildContext("UnusedX"),
            [new DataPoint(1, 999)]);
        var unusedYData = new SeriesData(
            "Unused Y",
            BuildContext("UnusedY"),
            [new DataPoint(1, 999)]);

        var factory = new Mock<IDataProviderFactory>();
        factory.Setup(f => f.ReadAsync(xSource, It.IsAny<CancellationToken>()))
            .ReturnsAsync([xData, unusedXData]);
        factory.Setup(f => f.ReadAsync(ySource, It.IsAny<CancellationToken>()))
            .ReturnsAsync([yData, unusedYData]);
        var converter = new OxyPlotConverter(
            factory.Object,
            Mock.Of<ILogger<OxyPlotConverter>>());

        OxyScatterSeries oxySeries = Assert.IsType<OxyScatterSeries>(
            Assert.Single(await converter.ToOxySeriesAsync(
                series,
                new StyleContext(1),
                CancellationToken.None)));

        Assert.Collection(
            oxySeries.Points,
            point =>
            {
                Assert.Equal(10, point.X);
                Assert.Equal(100, point.Y);
            });
    }

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

    private static SeriesContext BuildContext(string layer, int? patch = 2)
    {
        return new SeriesContext(
            "experiment",
            "simulation",
            new Gridcell(-33.1, 151.2, "site"),
            layer,
            stand: 1,
            patch: patch,
            individual: 3,
            pft: "TrBE");
    }
}
