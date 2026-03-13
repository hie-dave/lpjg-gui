using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identities;
using LpjGuess.Core.Models.Graphing.Style.Identifiers;
using LpjGuess.Core.Models.Graphing.Style.Providers;
using LpjGuess.Core.Models.Graphing.Style.Strategies;

namespace LpjGuess.Tests.Core.Models.Graphing.Style;

public class GraphingStyleTests
{
    private sealed class StubSeriesData : ISeriesData
    {
        public SeriesContext Context { get; }

        public StubSeriesData(SeriesContext context)
        {
            Context = context;
        }
    }

    private sealed class StubPalette : IColourPalette
    {
        public double LastPosition { get; private set; }
        private readonly Colour colour;

        public StubPalette(Colour colour)
        {
            this.colour = colour;
        }

        public Colour GetColour(double position)
        {
            LastPosition = position;
            return colour;
        }
    }

    private static SeriesContext BuildContext(
        string experiment = "exp",
        string simulation = "sim",
        string layer = "layer",
        int? stand = 1,
        int? patch = 2,
        int? individual = 3,
        string? pft = "TrBE")
    {
        return new SeriesContext(
            experiment,
            simulation,
            new Gridcell(-33.1, 151.2, "SiteA"),
            layer,
            stand,
            patch,
            individual,
            pft);
    }

    [Fact]
    public void SeriesContext_EqualityAndHashCode_RespectAllFields()
    {
        var a = BuildContext();
        var b = BuildContext();
        var different = BuildContext(layer: "other");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, different);
        Assert.False(a.Equals("not-context"));
    }

    [Fact]
    public void IdentityClasses_EqualityAndToString_Work()
    {
        var stringA = new StringIdentity("abc");
        var stringB = new StringIdentity("abc");
        var stringC = new StringIdentity("xyz");
        Assert.True(stringA.Equals(stringB));
        Assert.False(stringA.Equals(stringC));
        Assert.Equal("abc", stringA.ToString());

        var numericA = new NumericIdentity(7);
        var numericB = new NumericIdentity(7);
        var numericC = new NumericIdentity(8);
        Assert.True(numericA.Equals(numericB));
        Assert.False(numericA.Equals(numericC));
        Assert.Equal("7", numericA.ToString());

        var gridA = new GridcellIdentity(10.001, 20.001, "A");
        var gridB = new GridcellIdentity(10.002, 20.002, "B");
        var gridC = new GridcellIdentity(10.1, 20.1, "C");
        Assert.True(gridA.Equals(gridB));
        Assert.False(gridA.Equals(gridC));
        Assert.Equal("A", gridA.ToString());
    }

    [Fact]
    public void IdentifierClasses_IdentifyExpectedValues()
    {
        SeriesContext context = BuildContext();

        Assert.Equal(new StringIdentity("exp"), new ExperimentIdentifier().Identify(context));
        Assert.Equal(new StringIdentity("sim"), new SimulationIdentifier().Identify(context));
        Assert.Equal(new StringIdentity("layer"), new LayerIdentifier().Identify(context));
        Assert.Equal(new NumericIdentity(1), new StandIdentifier().Identify(context));
        Assert.Equal(new NumericIdentity(2), new PatchIdentifier().Identify(context));
        Assert.Equal(new NumericIdentity(3), new IndividualIdentifier().Identify(context));
        Assert.Equal(new StringIdentity("TrBE"), new PftIdentifier().Identify(context));

        SeriesIdentityBase gridIdentity = new GridcellIdentifier().Identify(context);
        Assert.IsType<GridcellIdentity>(gridIdentity);
        Assert.Equal(StyleVariationStrategy.ByGridcell, new GridcellIdentifier().GetStrategy());
    }

    [Fact]
    public void Identifiers_ThrowWhenContextMissingRequiredValue()
    {
        Assert.Throws<InvalidOperationException>(() => new StandIdentifier().Identify(BuildContext(stand: null)));
        Assert.Throws<InvalidOperationException>(() => new PatchIdentifier().Identify(BuildContext(patch: null)));
        Assert.Throws<InvalidOperationException>(() => new IndividualIdentifier().Identify(BuildContext(individual: null)));
        Assert.Throws<InvalidOperationException>(() => new PftIdentifier().Identify(BuildContext(pft: null)));

        SeriesContext noSimulation = BuildContext(simulation: null!);
        Assert.Throws<InvalidOperationException>(() => new SimulationIdentifier().Identify(noSimulation));
    }

    [Fact]
    public void SeriesIdentifier_ProducesIncrementingIdentities()
    {
        var identifier = new SeriesIdentifier();
        SeriesContext context = BuildContext();

        Assert.Equal(new NumericIdentity(0), identifier.Identify(context));
        Assert.Equal(new NumericIdentity(1), identifier.Identify(context));
    }

    [Fact]
    public void DataFilter_FiltersMatchingIdentityOnly()
    {
        var filter = new DataFilter(
            StyleVariationStrategy.ByLayer,
            [new StringIdentity("hidden")]);

        Assert.True(filter.IsFiltered(BuildContext(layer: "hidden")));
        Assert.False(filter.IsFiltered(BuildContext(layer: "visible")));
    }

    [Fact]
    public void FixedStyleProvider_AlwaysReturnsSameStyle()
    {
        var provider = new FixedStyleProvider<Colour>(Colours.Red);
        var style = provider.GetStyle(new StubSeriesData(BuildContext()));

        Assert.Equal(Colours.Red, style);
        Assert.Equal(StyleVariationStrategy.Fixed, provider.GetStrategy());

        provider.Initialize(10);
        provider.Reset();
        Assert.Equal(Colours.Red, provider.GetStyle(new StubSeriesData(BuildContext())));
    }

    [Fact]
    public void DynamicStyleProvider_CachesStylePerIdentity_AndResetClearsCache()
    {
        var provider = new DynamicStyleProvider<LegendPosition>(
            new LayerIdentifier(),
            new EnumStrategy<LegendPosition>());

        provider.Initialize(5);

        var first = provider.GetStyle(new StubSeriesData(BuildContext(layer: "A")));
        var secondForSame = provider.GetStyle(new StubSeriesData(BuildContext(layer: "A")));
        var thirdForDifferent = provider.GetStyle(new StubSeriesData(BuildContext(layer: "B")));

        Assert.Equal(first, secondForSame);
        Assert.NotEqual(first, thirdForDifferent);

        provider.Reset();
        var afterReset = provider.GetStyle(new StubSeriesData(BuildContext(layer: "B")));
        Assert.Equal(first, afterReset);
    }

    [Fact]
    public void ColourStrategy_UsesFinitePaletteAndFallbackWhenNeeded()
    {
        var fallback = new StubPalette(Colours.Green);
        var strategy = new ColourStrategy([Colours.Red], fallback);

        strategy.Initialise(1);
        Assert.Equal(Colours.Red, strategy.GetValue(0));

        strategy.Initialise(10);
        Colour fallbackColour = strategy.GetValue(5);
        Assert.Equal(Colours.Green, fallbackColour);
        Assert.Equal(0.5, fallback.LastPosition, 3);
    }
}
