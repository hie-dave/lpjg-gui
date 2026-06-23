using LpjGuess.Core.Interfaces;
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
    private sealed class GraphContainer
    {
        public Graph? Graph { get; set; }
    }

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
    public void ScatterSeries_IndependentSources_UsesValueAxesFromEachSource()
    {
        var xSource = new ModelOutput("file_lai", "Date", ["Total"], []);
        var ySource = new ModelOutput("file_anpp", "Date", ["Total"], []);
        var series = new ScatterSeries(
            "Comparison",
            new FixedStyleProvider<Colour>(Colours.Blue),
            xSource,
            ySource,
            AxisPosition.Bottom,
            AxisPosition.Left);

        AxisRequirements[] requirements = series.GetAxisRequirements().ToArray();

        Assert.Equal(xSource.GetYAxisType(), requirements[0].Type);
        Assert.Equal(xSource.GetYAxisTitle(), requirements[0].Title);
        Assert.Equal(ySource.GetYAxisType(), requirements[1].Type);
        Assert.Equal(ySource.GetYAxisTitle(), requirements[1].Title);
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
        Assert.Null(series.XDataSource);
        Assert.IsType<ModelOutput>(series.YDataSource);
    }

    [Fact]
    public void ScatterSeries_IndependentSources_RoundTripThroughJson()
    {
        var graph = new Graph(
            "Comparison",
            [
                new ScatterSeries(
                    "Points",
                    new FixedStyleProvider<Colour>(Colours.Green),
                    new ModelOutput("file_lai", "Date", ["Total"], []),
                    new ModelOutput("file_anpp", "Date", ["Total"], []),
                    AxisPosition.Bottom,
                    AxisPosition.Left)
            ]);

        Graph clone = JsonSerialisation.DeepClone(graph);

        ScatterSeries series = Assert.IsType<ScatterSeries>(Assert.Single(clone.Series));
        Assert.Equal("file_lai", Assert.IsType<ModelOutput>(series.XDataSource).OutputFileType);
        Assert.Equal("file_anpp", Assert.IsType<ModelOutput>(series.YDataSource).OutputFileType);
    }

    [Fact]
    public void LineSeries_IndependentSources_RoundTripThroughJson()
    {
        var graph = new Graph(
            "Comparison",
            [
                new LineSeries(
                    "Line",
                    new FixedStyleProvider<Colour>(Colours.Blue),
                    new ModelOutput("file_lai", "Date", ["Total"], []),
                    new ModelOutput("file_anpp", "Date", ["Total"], []),
                    AxisPosition.Bottom,
                    AxisPosition.Left,
                    new FixedStyleProvider<LineType>(LineType.Dashed),
                    new FixedStyleProvider<LineThickness>(LineThickness.Thick))
            ]);

        Graph clone = JsonSerialisation.DeepClone(graph);

        LineSeries series = Assert.IsType<LineSeries>(Assert.Single(clone.Series));
        Assert.Equal("file_lai", Assert.IsType<ModelOutput>(series.XDataSource).OutputFileType);
        Assert.Equal("file_anpp", Assert.IsType<ModelOutput>(series.YDataSource).OutputFileType);
        Assert.Equal(LineType.Dashed, Assert.IsType<FixedStyleProvider<LineType>>(series.Type).Style);
        Assert.Equal(LineThickness.Thick, Assert.IsType<FixedStyleProvider<LineThickness>>(series.Thickness).Style);
    }

    [Fact]
    public void LegacySingleDataSourceJson_RemainsSupported()
    {
        var graph = new Graph(
            "Legacy",
            [
                new ScatterSeries(
                    "Points",
                    new FixedStyleProvider<Colour>(Colours.Green),
                    new ModelOutput("file_lai", "Date", ["Total"], []),
                    AxisPosition.Bottom,
                    AxisPosition.Left)
            ]);
        using TempFile file = TempFile.Create(ext: "json");
        new GraphContainer { Graph = graph }.SerialiseTo(file.AbsolutePath);
        string legacyJson = File.ReadAllText(file.AbsolutePath)
            .Replace("\"YDataSource\":", "\"DataSource\":");
        File.WriteAllText(file.AbsolutePath, legacyJson);

        Graph restored = JsonSerialisation
            .DeserialiseFrom<GraphContainer>(file.AbsolutePath)
            .Graph!;

        ScatterSeries series = Assert.IsType<ScatterSeries>(Assert.Single(restored.Series));
        Assert.Null(series.XDataSource);
        Assert.Equal("file_lai", Assert.IsType<ModelOutput>(series.YDataSource).OutputFileType);
    }

    [Fact]
    public void DataSource_CanBeClonedThroughInterfaceForIndependentXAxis()
    {
        IDataSource source = new ModelOutput("file_lai", "Date", ["Total"], []);

        IDataSource clone = JsonSerialisation.DeepCloneRuntimeType(source);

        Assert.NotSame(source, clone);
        Assert.Equal("file_lai", Assert.IsType<ModelOutput>(clone).OutputFileType);
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
