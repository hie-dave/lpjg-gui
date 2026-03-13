using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Core.Models.Factorial.Generators.Values;
using LpjGuess.Core.Parsers;

namespace LpjGuess.Tests.Core.Models.Factorial;

public class FactorialBehaviourTests
{
    [Fact]
    public void RangeGenerator_GeneratesExpectedRangeAndCount()
    {
        var generator = new RangeGenerator<int>(2, 4, 3);

        Assert.Equal(new[] { 2, 5, 8, 11 }, generator.Generate().ToArray());
        Assert.Equal(4, generator.NumValues());
    }

    [Fact]
    public void DiscreteValues_GeneratesInputValues()
    {
        var values = new DiscreteValues<string>(["a", "b"]);

        Assert.Equal(new[] { "a", "b" }, values.Generate().ToArray());
        Assert.Equal(2, values.NumValues());
    }

    [Fact]
    public void TopLevelAndBlockGenerators_ProduceExpectedFactorTypes()
    {
        var top = new TopLevelFactorGenerator("npatch", new DiscreteValues<int>([1, 2]));
        var block = new BlockFactorGenerator("pft", "TeBE", "include", new DiscreteValues<int>([0, 1]));

        IFactor[] topFactors = top.Generate().ToArray();
        IFactor[] blockFactors = block.Generate().ToArray();

        Assert.All(topFactors, f => Assert.IsType<TopLevelParameter>(f));
        Assert.All(blockFactors, f => Assert.IsType<BlockParameter>(f));
        Assert.Equal(2, top.NumFactors());
        Assert.Equal(2, block.NumFactors());
    }

    [Fact]
    public void FactorialGenerator_ReturnsEmpty_WhenNoFactorsConfigured()
    {
        var generator = new FactorialGenerator(false, Array.Empty<IFactorGenerator>());
        Assert.Empty(generator.Generate());
    }

    [Fact]
    public void FactorialGenerator_BuildsFullFactorial_WhenEnabled()
    {
        var a = new SimpleFactorGenerator("A", [new TopLevelParameter("a", "1"), new TopLevelParameter("a", "2")]);
        var b = new SimpleFactorGenerator("B", [new TopLevelParameter("b", "x"), new TopLevelParameter("b", "y")]);

        var generator = new FactorialGenerator(true, [a, b]);
        ISimulation[] simulations = generator.Generate().ToArray();

        Assert.Equal(4, simulations.Length);
        Assert.Contains(simulations, s => s.Name == "a-1_b-x");
        Assert.Contains(simulations, s => s.Name == "a-1_b-y");
        Assert.Contains(simulations, s => s.Name == "a-2_b-x");
        Assert.Contains(simulations, s => s.Name == "a-2_b-y");
    }

    [Fact]
    public void CompositeFactor_AppliesAllChangesAndAggregatesNames()
    {
        string content = """
            npatch 1
            outputdirectory "old"
            """;

        var parser = new InstructionFileParser(content, "/tmp/test.ins");
        var composite = new CompositeFactor([
            new TopLevelParameter("npatch", "7"),
            new TopLevelParameter("outputdirectory", "\"new\"")
        ]);

        composite.Apply(parser);

        Assert.Equal("npatch-7_outputdirectory-\"new\"", composite.GetName());
        Assert.Equal(2, composite.GetChanges().Count());
        Assert.Equal(7, parser.GetTopLevelParameter("npatch")!.AsInt());
        Assert.Equal("new", parser.GetTopLevelParameter("outputdirectory")!.AsString());
    }

    [Fact]
    public void SimulationGenerate_AppliesFactorAndPreservesPfts_WhenNoSelectionProvided()
    {
        using TempDirectory temp = TempDirectory.Create();
        string insFile = Path.Combine(temp.AbsolutePath, "base.ins");
        string target = Path.Combine(temp.AbsolutePath, "generated.ins");

        File.WriteAllText(insFile, """
            npatch 1

            pft "A" (
                include 1
            )

            pft "B" (
                include 1
            )
            """);

        var simulation = new Simulation([new TopLevelParameter("npatch", "9")]);
        simulation.Generate(insFile, target, Array.Empty<string>());

        var parser = InstructionFileParser.FromFile(target);
        Assert.Equal(9, parser.GetTopLevelParameter("npatch")!.AsInt());
        Assert.Equal(1, parser.GetBlockParameter("pft", "A", "include")!.AsInt());
        Assert.Equal(1, parser.GetBlockParameter("pft", "B", "include")!.AsInt());
    }

    [Fact]
    public void ExperimentCreateBaseline_ReturnsExpectedDefaults()
    {
        Experiment experiment = Experiment.CreateBaseline();

        Assert.Equal("Baseline", experiment.Name);
        Assert.Equal("baseline", experiment.Runner);
        Assert.Empty(experiment.DisabledInsFiles);
        Assert.Empty(experiment.Pfts);
        Assert.Single(experiment.SimulationGenerator.Generate());
    }
}
