using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Providers;

namespace LpjGuess.Tests.Core.Models.Graphing;

public class GraphingCoreTests
{
    private sealed class StubDataSource : IDataSource
    {
        public AxisType GetXAxisType() => AxisType.Linear;
        public AxisType GetYAxisType() => AxisType.Linear;
        public string GetXAxisTitle() => "X";
        public string GetYAxisTitle() => "Y";
        public string GetName() => "Stub";
        public IEnumerable<StyleVariationStrategy> GetAllowedStyleVariationStrategies() => [StyleVariationStrategy.Fixed];
    }

    private sealed class StubSeries : ISeries
    {
        public string Title { get; set; } = "S";
        public IStyleProvider<Colour> ColourProvider { get; set; } = new FixedStyleProvider<Colour>(Colours.Blue);
        public IDataSource DataSource { get; set; } = new StubDataSource();
        public AxisPosition XAxisPosition { get; set; } = AxisPosition.Bottom;
        public AxisPosition YAxisPosition { get; set; } = AxisPosition.Left;
        private readonly AxisRequirements[] requirements;

        public StubSeries(params AxisRequirements[] requirements)
        {
            this.requirements = requirements;
        }

        public IEnumerable<AxisRequirements> GetAxisRequirements() => requirements;
    }

    [Fact]
    public void Colour_FromHex_ParsesSupportedFormats()
    {
        Assert.Equal(new Colour(0x11, 0x22, 0x33), Colour.FromHex("#123"));
        Assert.Equal(new Colour(0x11, 0x22, 0x33, 0x44), Colour.FromHex("#1234"));
        Assert.Equal(new Colour(0x12, 0xAB, 0xEF), Colour.FromHex("12abef"));
        Assert.Equal(new Colour(0x12, 0xAB, 0xEF, 0x10), Colour.FromHex("#12abef10"));
    }

    [Theory]
    [InlineData(0, 255, 0, 0)]
    [InlineData(60, 255, 255, 0)]
    [InlineData(120, 0, 255, 0)]
    [InlineData(180, 0, 255, 255)]
    [InlineData(240, 0, 0, 255)]
    [InlineData(300, 255, 0, 255)]
    public void Colour_FromHsv_CoversHueSectors(double hue, byte r, byte g, byte b)
    {
        Colour colour = Colour.FromHsv(hue, 1, 1);
        Assert.Equal(new Colour(r, g, b), colour);
    }

    [Fact]
    public void Colour_FromHsv_ClampsAndNormalizesInputs()
    {
        Colour wrapped = Colour.FromHsv(-120, 2.0, 2.0);
        Assert.Equal(255, wrapped.A);
        Assert.True(wrapped.R <= 255 && wrapped.G <= 255 && wrapped.B <= 255);
    }

    [Fact]
    public void Colour_ToStringAndEquality_WorkAsExpected()
    {
        Colour opaque = new(16, 32, 48);
        Colour withAlpha = new(16, 32, 48, 64);

        Assert.Equal("#102030", opaque.ToString());
        Assert.Equal("#10203040", withAlpha.ToString());
        Assert.True(opaque == new Colour(16, 32, 48));
        Assert.True(opaque != withAlpha);
        Assert.False(opaque.Equals("not-colour"));
    }

    [Fact]
    public void AxisRequirements_CompatibilityAndAxisHelpers_Work()
    {
        var leftLinear = new AxisRequirements(AxisType.Linear, AxisPosition.Left, "Y");
        var rightLinear = new AxisRequirements(AxisType.Linear, AxisPosition.Right, "Y2");
        var leftDate = new AxisRequirements(AxisType.DateTime, AxisPosition.Left, "Date");

        leftLinear.AssertCompatibility(rightLinear);
        Assert.Throws<InvalidOperationException>(() => leftLinear.AssertCompatibility(leftDate));

        Assert.True(leftLinear.IsYAxis());
        Assert.False(leftLinear.IsXAxis());
        var top = new AxisRequirements(AxisType.Linear, AxisPosition.Top, "X2");
        Assert.True(top.IsXAxis());
        Assert.False(top.IsYAxis());
    }

    [Fact]
    public void Legend_DefaultAndExplicitConstructors_Work()
    {
        var defaultLegend = new Legend();
        Assert.True(defaultLegend.Visible);
        Assert.Equal(LegendPosition.TopLeft, defaultLegend.Position);
        Assert.Equal(Colours.Transparent, defaultLegend.BackgroundColour);

        var explicitLegend = new Legend(false, LegendPosition.BottomCenter, LegendPlacement.Outside,
            LegendOrientation.Horizontal, Colours.White, Colours.Black);
        Assert.False(explicitLegend.Visible);
        Assert.Equal(LegendPosition.BottomCenter, explicitLegend.Position);
        Assert.Equal(LegendPlacement.Outside, explicitLegend.Placement);
        Assert.Equal(LegendOrientation.Horizontal, explicitLegend.Orientation);
        Assert.Equal(Colours.White, explicitLegend.BackgroundColour);
        Assert.Equal(Colours.Black, explicitLegend.BorderColour);
    }

    [Fact]
    public void RainbowPalette_ReturnsDeterministicColours()
    {
        var palette = new RainbowPalette();
        Colour first = palette.GetColour(0);
        Colour second = palette.GetColour(0);
        Colour other = palette.GetColour(0.5);

        Assert.Equal(first, second);
        Assert.NotEqual(first, other);
    }

    [Fact]
    public void Graph_GetAxisRequirements_FlattensSeriesRequirements()
    {
        AxisRequirements axis1 = new(AxisType.Linear, AxisPosition.Left, "Y");
        AxisRequirements axis2 = new(AxisType.DateTime, AxisPosition.Bottom, "Date");
        var graph = new Graph("Graph", [new StubSeries(axis1), new StubSeries(axis2)]);

        AxisRequirements[] requirements = graph.GetAxisRequirements().ToArray();
        Assert.Equal(2, requirements.Length);
        Assert.Contains(axis1, requirements);
        Assert.Contains(axis2, requirements);
    }
}
