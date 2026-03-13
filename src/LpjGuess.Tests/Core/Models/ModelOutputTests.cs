using System.Reflection;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Entities;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Core.Models.Importer;
using LpjGuess.Core.Services;

namespace LpjGuess.Tests.Core.Models;

public class ModelOutputTests
{
    private sealed class NeverFilter : IDataFilter
    {
        public bool IsFiltered(SeriesContext context) => false;
    }

    [Fact]
    public void Constructor_DefaultsAreInitialised()
    {
        var model = new ModelOutput();

        Assert.Equal(string.Empty, model.OutputFileType);
        Assert.Equal(string.Empty, model.XAxisColumn);
        Assert.Empty(model.YAxisColumns);
        Assert.Empty(model.Filters);
    }

    [Fact]
    public void Constructor_WithArguments_AssignsValues()
    {
        var filters = new IDataFilter[] { new NeverFilter() };
        var model = new ModelOutput("file_lai", "Date", ["TeBE"], filters);

        Assert.Equal("file_lai", model.OutputFileType);
        Assert.Equal("Date", model.XAxisColumn);
        Assert.Equal(new[] { "TeBE" }, model.YAxisColumns.ToArray());
        Assert.Single(model.Filters);
    }

    [Theory]
    [InlineData("Date", AxisType.DateTime)]
    [InlineData("Lon", AxisType.Linear)]
    public void GetXAxisType_ReturnsExpectedType(string column, AxisType expected)
    {
        var model = new ModelOutput("file_lai", column, ["TeBE"], []);
        Assert.Equal(expected, model.GetXAxisType());
    }

    [Fact]
    public void GetYAxisType_IsAlwaysLinear()
    {
        var model = new ModelOutput("file_lai", "Date", ["TeBE"], []);
        Assert.Equal(AxisType.Linear, model.GetYAxisType());
    }

    [Fact]
    public void GetName_FallsBackToOutputFileType_ForUnknownTypes()
    {
        var model = new ModelOutput("unknown_type", "Date", ["A"], []);
        Assert.Equal("unknown_type", model.GetName());
    }

    [Fact]
    public void GetYAxisTitle_HandlesSingleAndZeroUnitsBranches()
    {
        var single = new ModelOutput("file_lai", "Date", ["TeBE"], []);
        string oneUnitTitle = single.GetYAxisTitle();
        Assert.Contains("LAI", oneUnitTitle);
        Assert.Contains("(m2/m2)", oneUnitTitle);

        var zero = new ModelOutput("file_lai", "Date", [], []);
        string zeroUnitTitle = zero.GetYAxisTitle();
        Assert.Equal("LAI (various units)", zeroUnitTitle);
    }

    [Fact]
    public void GetYAxisTitle_UsesNameOnly_WhenMultipleUnitsSelected()
    {
        (string fileType, string[] layers) = FindStaticFileTypeWithDifferentUnits();
        var model = new ModelOutput(fileType, "Date", layers, []);

        string title = model.GetYAxisTitle();
        string expectedName = OutputFileDefinitions.GetMetadata(fileType).Name;

        Assert.Equal(expectedName, title);
    }

    [Fact]
    public void GetAllowedStyleVariationStrategies_FollowsAggregationLevel()
    {
        string gridType = FindFileTypeByLevel(AggregationLevel.Gridcell);
        string patchType = FindFileTypeByLevel(AggregationLevel.Patch);
        string indivType = FindFileTypeByLevel(AggregationLevel.Individual);

        var grid = new ModelOutput(gridType, "Date", ["X"], []);
        var patch = new ModelOutput(patchType, "Date", ["X"], []);
        var indiv = new ModelOutput(indivType, "Date", ["X"], []);

        var gridStrategies = grid.GetAllowedStyleVariationStrategies().ToHashSet();
        var patchStrategies = patch.GetAllowedStyleVariationStrategies().ToHashSet();
        var indivStrategies = indiv.GetAllowedStyleVariationStrategies().ToHashSet();

        Assert.Contains(StyleVariationStrategy.ByExperiment, gridStrategies);
        Assert.Contains(StyleVariationStrategy.BySimulation, gridStrategies);
        Assert.Contains(StyleVariationStrategy.BySeries, gridStrategies);
        Assert.Contains(StyleVariationStrategy.ByLayer, gridStrategies);
        Assert.Contains(StyleVariationStrategy.ByGridcell, gridStrategies);
        Assert.DoesNotContain(StyleVariationStrategy.ByPatch, gridStrategies);

        Assert.Contains(StyleVariationStrategy.ByGridcell, patchStrategies);
        Assert.Contains(StyleVariationStrategy.ByStand, patchStrategies);
        Assert.Contains(StyleVariationStrategy.ByPatch, patchStrategies);
        Assert.DoesNotContain(StyleVariationStrategy.ByIndividual, patchStrategies);

        Assert.Contains(StyleVariationStrategy.ByIndividual, indivStrategies);
        Assert.Contains(StyleVariationStrategy.ByPft, indivStrategies);
    }

    private static string FindFileTypeByLevel(AggregationLevel level)
    {
        string? fileType = OutputFileDefinitions
            .GetAllFileTypes()
            .FirstOrDefault(ft => OutputFileDefinitions.GetMetadata(ft).Level == level);

        return fileType ?? throw new InvalidOperationException($"No output file type found for level {level}");
    }

    private static (string fileType, string[] layers) FindStaticFileTypeWithDifferentUnits()
    {
        foreach (string fileType in OutputFileDefinitions.GetAllFileTypes())
        {
            OutputFileMetadata metadata = OutputFileDefinitions.GetMetadata(fileType);
            if (metadata.Layers is not StaticLayers staticLayers)
                continue;

            var field = typeof(StaticLayers).GetField("layers", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?? throw new InvalidOperationException("Unable to find static layers backing field");

            var layers = (IReadOnlyDictionary<string, Unit>)field.GetValue(staticLayers)!;
            var groups = layers.GroupBy(kvp => kvp.Value.Name).Where(g => g.Any()).ToList();
            if (groups.Count > 1)
            {
                string first = groups[0].First().Key;
                string second = groups[1].First().Key;
                return (fileType, [first, second]);
            }
        }

        throw new InvalidOperationException("No static output file type with multiple units was found");
    }
}
