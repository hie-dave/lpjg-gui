using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Core.Models.Factorial.Generators.Values;
using LpjGuess.Core.Parsers;
using LpjGuess.Core.Services;
using LpjGuess.Core.Extensions;

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
    public void FactorialGenerator_DoesNotBuildFullFactorial_WhenDisabled()
    {
        var a = new SimpleFactorGenerator("A", [new TopLevelParameter("a", "1"), new TopLevelParameter("a", "2")]);
        var b = new SimpleFactorGenerator("B", [new TopLevelParameter("b", "x"), new TopLevelParameter("b", "y")]);

        var generator = new FactorialGenerator(false, [a, b]);
        ISimulation[] simulations = generator.Generate().ToArray();

        Assert.Equal(4, simulations.Length);
        Assert.Contains(simulations, s => s.Name == "a-1");
        Assert.Contains(simulations, s => s.Name == "a-2");
        Assert.Contains(simulations, s => s.Name == "b-x");
        Assert.Contains(simulations, s => s.Name == "b-y");
    }

    [Fact]
    public void CompositeFactor_AppliesAllChangesAndAggregatesNames()
    {
        string content = """
            npatch 1
            outputdirectory "old"
            """;

        using TempDirectory temp = TempDirectory.Create(GetType().Name);
        string path = Path.Combine(temp.AbsolutePath, "test.ins");
        var parser = new InstructionFileParser(content, path);
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

    [Fact]
    public void ParameterOverrides_PreserveBlockIdentity()
    {
        var factor = new CompositeFactor([
            new TopLevelParameter("npatch", "4"),
            new BlockParameter("pft", "TeBE", "sla", "27")
        ]);

        ParameterOverride[] changes = factor.GetParameterOverrides().ToArray();

        Assert.Equal("npatch", changes[0].Target.DisplayName);
        Assert.Equal("pft[TeBE].sla", changes[1].Target.DisplayName);
    }

    [Fact]
    public void ExperimentDesignAnalyser_CalculatesSimulationAndRunCounts()
    {
        var generator = new FactorialGenerator(true, [
            new TopLevelFactorGenerator("npatch", new DiscreteValues<int>([1, 2])),
            new BlockFactorGenerator("pft", "TeBE", "sla", new DiscreteValues<int>([20, 25, 30]))
        ]);

        ExperimentDesignAnalysis analysis = ExperimentDesignAnalyser.Analyse(generator, 4);

        Assert.True(analysis.IsValid);
        Assert.Equal(6, analysis.SimulationCount);
        Assert.Equal(24, analysis.ModelRunCount);
    }

    [Fact]
    public void ExperimentDesignAnalyser_DetectsConflictingCombinedTargets()
    {
        var generator = new FactorialGenerator(true, [
            new TopLevelFactorGenerator("npatch", new DiscreteValues<int>([1, 2])),
            new TopLevelFactorGenerator("npatch", new DiscreteValues<int>([3, 4]))
        ]);

        ExperimentDesignAnalysis analysis = ExperimentDesignAnalyser.Analyse(generator, 1);

        Assert.False(analysis.IsValid);
        Assert.Contains(analysis.Issues, issue => issue.Message.Contains("npatch"));
    }

    [Fact]
    public void ExperimentDesignAnalyser_DetectsDuplicateTargetsInsideScenario()
    {
        var generator = new FactorialGenerator(false, [
            new SimpleFactorGenerator("Climate scenarios", [
                new CompositeFactor([
                    new TopLevelParameter("co2", "400"),
                    new TopLevelParameter("co2", "550")
                ])
            ])
        ]);

        ExperimentDesignAnalysis analysis = ExperimentDesignAnalyser.Analyse(generator, 1);

        Assert.False(analysis.IsValid);
        Assert.Contains(analysis.Issues, issue => issue.Message.Contains("more than once"));
    }

    [Fact]
    public void CompositeFactor_UsesOptionalScenarioName()
    {
        var scenario = new CompositeFactor([
            new TopLevelParameter("co2", "550"),
            new TopLevelParameter("temperature", "2")
        ]) { Name = "Moderate warming" };

        Assert.Equal("Moderate warming", scenario.GetName());
    }

    [Fact]
    public void InstructionFileAnalyser_TreatsUnknownParametersAsInformation()
    {
        using TempDirectory temp = TempDirectory.Create();
        string file = Path.Combine(temp.AbsolutePath, "base.ins");
        File.WriteAllText(file, """
            npatch 1
            pft "TeBE" (
                include 1
            )
            """);
        var generator = new FactorialGenerator(false, [
            new TopLevelFactorGenerator("missing", new DiscreteValues<int>([1]))
        ]);

        IReadOnlyList<ExperimentDesignIssue> issues =
            ExperimentInstructionFileAnalyser.Analyse(
                generator,
                [file],
                ["TeBS"]);

        Assert.Contains(issues, issue =>
            issue.Severity == ExperimentDesignIssueSeverity.Information &&
            issue.Message.Contains("'missing'"));
        Assert.Contains(issues, issue =>
            issue.Severity == ExperimentDesignIssueSeverity.Error &&
            issue.Message.Contains("TeBS"));
    }

    [Fact]
    public void InstructionFileAnalyser_TreatsMissingBlockAsError()
    {
        using TempDirectory temp = TempDirectory.Create();
        string file = Path.Combine(temp.AbsolutePath, "base.ins");
        File.WriteAllText(file, "npatch 1");
        var generator = new FactorialGenerator(false, [
            new BlockFactorGenerator(
                "pft",
                "TeBE",
                "sla",
                new DiscreteValues<int>([27]))
        ]);

        IReadOnlyList<ExperimentDesignIssue> issues =
            ExperimentInstructionFileAnalyser.Analyse(generator, [file], []);

        Assert.Contains(issues, issue =>
            issue.Severity == ExperimentDesignIssueSeverity.Error &&
            issue.Message.Contains("pft[TeBE]"));
    }

    [Fact]
    public void InstructionFileAnalyser_DoesNotRejectInheritedBlockParameter()
    {
        using TempDirectory temp = TempDirectory.Create();
        string file = Path.Combine(temp.AbsolutePath, "base.ins");
        File.WriteAllText(file, """
            group "tree" (
                sla 27
            )
            pft "TeBE" (
                tree
                include 1
            )
            """);
        var generator = new FactorialGenerator(false, [
            new BlockFactorGenerator(
                "pft",
                "TeBE",
                "sla",
                new DiscreteValues<int>([30]))
        ]);

        IReadOnlyList<ExperimentDesignIssue> issues =
            ExperimentInstructionFileAnalyser.Analyse(generator, [file], []);

        Assert.DoesNotContain(
            issues,
            issue => issue.Severity == ExperimentDesignIssueSeverity.Error);
        Assert.Contains(
            issues,
            issue => issue.Severity == ExperimentDesignIssueSeverity.Information &&
                     issue.Message.Contains("inherited"));
    }
}
