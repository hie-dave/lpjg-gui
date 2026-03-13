using LpjGuess.Core.Extensions;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style.Identifiers;

namespace LpjGuess.Tests.Core.Extensions;

public class StyleVariationStrategyExtensionsTests
{
    [Theory]
    [InlineData(StyleVariationStrategy.ByExperiment, typeof(ExperimentIdentifier))]
    [InlineData(StyleVariationStrategy.ByGridcell, typeof(GridcellIdentifier))]
    [InlineData(StyleVariationStrategy.BySimulation, typeof(SimulationIdentifier))]
    [InlineData(StyleVariationStrategy.ByStand, typeof(StandIdentifier))]
    [InlineData(StyleVariationStrategy.ByPatch, typeof(PatchIdentifier))]
    [InlineData(StyleVariationStrategy.ByIndividual, typeof(IndividualIdentifier))]
    [InlineData(StyleVariationStrategy.ByPft, typeof(PftIdentifier))]
    [InlineData(StyleVariationStrategy.BySeries, typeof(SeriesIdentifier))]
    [InlineData(StyleVariationStrategy.ByLayer, typeof(LayerIdentifier))]
    public void CreateIdentifier_ReturnsExpectedIdentifier(StyleVariationStrategy strategy, Type expectedType)
    {
        object identifier = strategy.CreateIdentifier();
        Assert.IsType(expectedType, identifier);
    }

    [Fact]
    public void CreateIdentifier_ThrowsForUnsupportedStrategy()
    {
        Assert.Throws<ArgumentException>(() => StyleVariationStrategy.Fixed.CreateIdentifier());
    }
}
