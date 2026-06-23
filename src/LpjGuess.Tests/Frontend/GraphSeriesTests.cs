using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Series;
using LpjGuess.Core.Models.Graphing.Style.Providers;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Serialisation.Json;

namespace LpjGuess.Tests.Frontend;

public class GraphSeriesTests
{
    [Fact]
    public void CreateSeries_Points_CreatesScatterSeries()
    {
        ISeries series = GraphPresenter.CreateSeries(SeriesType.Points);

        Assert.IsType<ScatterSeries>(series);
        Assert.IsType<ModelOutput>(series.DataSource);
        Assert.Equal(AxisPosition.Bottom, series.XAxisPosition);
        Assert.Equal(AxisPosition.Left, series.YAxisPosition);
    }

    [Fact]
    public void ScatterSeries_ProvidesCommonAxisRequirements()
    {
        var source = new ModelOutput("file_lai", "Height", ["Total"], []);
        var series = new ScatterSeries(
            "Points",
            new FixedStyleProvider<Colour>(Colours.Blue),
            source,
            AxisPosition.Top,
            AxisPosition.Right);

        AxisRequirements[] requirements = series.GetAxisRequirements().ToArray();

        Assert.Collection(
            requirements,
            x =>
            {
                Assert.Equal(AxisType.Linear, x.Type);
                Assert.Equal(AxisPosition.Top, x.Position);
                Assert.Equal("Height", x.Title);
            },
            y =>
            {
                Assert.Equal(AxisType.Linear, y.Type);
                Assert.Equal(AxisPosition.Right, y.Position);
            });
    }

    [Fact]
    public void ScatterSeries_RoundTripsThroughJson()
    {
        var graph = new Graph(
            "Scatter plot",
            [
                new ScatterSeries(
                    "Points",
                    new FixedStyleProvider<Colour>(Colours.Green),
                    new ModelOutput("file_lai", "Height", ["Total"], []),
                    AxisPosition.Bottom,
                    AxisPosition.Left)
            ]);

        Graph clone = JsonSerialisation.DeepClone(graph);

        ScatterSeries series = Assert.IsType<ScatterSeries>(Assert.Single(clone.Series));
        Assert.Equal("Points", series.Title);
        Assert.Equal(Colours.Green, Assert.IsType<FixedStyleProvider<Colour>>(series.ColourProvider).Style);
        Assert.IsType<ModelOutput>(series.DataSource);
    }

    [Fact]
    public void ValidationFactory_SupportsScatterSeries()
    {
        var series = new ScatterSeries(
            "Points",
            new FixedStyleProvider<Colour>(Colours.Blue),
            new ModelOutput("file_lai", "Height", ["Total"], []),
            AxisPosition.Bottom,
            AxisPosition.Left);

        var command = new SeriesValidationCommandFactory().CreateValidationCommand(series);

        Assert.IsType<ScatterSeriesValidationCommand>(command);
    }
}
