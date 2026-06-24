using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace LpjGuess.Tests.Runner.Services;

public class ExistingOutputServiceTests
{
    private sealed class TestSimulation : ISimulation
    {
        public string Name { get; }
        public IEnumerable<IFactor> Changes => [];

        public TestSimulation(string name)
        {
            Name = name;
        }

        public void Generate(
            string insFile,
            string targetFile,
            IEnumerable<string> pfts)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class TestCatalog : IResultCatalog
    {
        private readonly SimulationIndex index;

        public TestCatalog(IEnumerable<string> simulations)
        {
            index = new SimulationIndex(simulations);
        }

        public void WriteIndex(IPathResolver pathResolver, SimulationIndex index)
        {
            throw new NotSupportedException();
        }

        public void WriteSimulation(
            string simulationDirectory,
            SimulationManifest manifest)
        {
            throw new NotSupportedException();
        }

        public SimulationManifest ReadManifest(string simulationDirectory)
        {
            throw new NotSupportedException();
        }

        public SimulationIndex ReadIndex(IPathResolver pathResolver)
        {
            return index;
        }
    }

    [Fact]
    public void Preserve_Does_Not_Delete_Existing_Indexed_Output()
    {
        using TempDirectory temp = TempDirectory.Create();
        SimulationBatch batch = CreateBatch(temp, ["base/sim-1"], ["sim-1"]);
        string directory = CreateOutputDirectory(temp, "base", "sim-1");

        CreateService().Apply(batch, ExistingOutputPolicy.Preserve);

        Assert.True(Directory.Exists(directory));
    }

    [Fact]
    public void CleanManaged_Deletes_Planned_Indexed_Output()
    {
        using TempDirectory temp = TempDirectory.Create();
        SimulationBatch batch = CreateBatch(temp, ["base/sim-1"], ["sim-1"]);
        string directory = CreateOutputDirectory(temp, "base", "sim-1");

        CreateService().Apply(batch, ExistingOutputPolicy.CleanManaged);

        Assert.False(Directory.Exists(directory));
    }

    [Fact]
    public void CleanManaged_Does_Not_Delete_Unindexed_Planned_Output()
    {
        using TempDirectory temp = TempDirectory.Create();
        SimulationBatch batch = CreateBatch(temp, [], ["sim-1"]);
        string directory = CreateOutputDirectory(temp, "base", "sim-1");

        CreateService().Apply(batch, ExistingOutputPolicy.CleanManaged);

        Assert.True(Directory.Exists(directory));
    }

    [Fact]
    public void PruneStale_Deletes_Indexed_Output_Not_In_Current_Plan()
    {
        using TempDirectory temp = TempDirectory.Create();
        SimulationBatch batch = CreateBatch(temp, ["base/old-sim"], ["sim-1"]);
        string staleDirectory = CreateOutputDirectory(temp, "base", "old-sim");
        string plannedDirectory = CreateOutputDirectory(temp, "base", "sim-1");

        CreateService().Apply(batch, ExistingOutputPolicy.PruneStale);

        Assert.False(Directory.Exists(staleDirectory));
        Assert.True(Directory.Exists(plannedDirectory));
    }

    [Fact]
    public void Fail_Throws_When_Planned_Output_Exists()
    {
        using TempDirectory temp = TempDirectory.Create();
        SimulationBatch batch = CreateBatch(temp, [], ["sim-1"]);
        CreateOutputDirectory(temp, "base", "sim-1");

        Assert.Throws<InvalidOperationException>(() =>
            CreateService().Apply(batch, ExistingOutputPolicy.Fail));
    }

    [Fact]
    public void Apply_Does_Not_Delete_Indexed_Path_Outside_Output_Root()
    {
        using TempDirectory output = TempDirectory.Create();
        using TempDirectory outside = TempDirectory.Create();
        SimulationBatch batch = CreateBatch(
            output,
            [outside.AbsolutePath],
            ["sim-1"]);

        CreateService().Apply(batch, ExistingOutputPolicy.PruneStale);

        Assert.True(Directory.Exists(outside.AbsolutePath));
    }

    private static ExistingOutputService CreateService()
    {
        return new ExistingOutputService(
            NullLogger<ExistingOutputService>.Instance);
    }

    private static SimulationBatch CreateBatch(
        TempDirectory temp,
        IEnumerable<string> indexedSimulations,
        IEnumerable<string> plannedSimulations)
    {
        string baseIns = Path.Combine(temp.AbsolutePath, "base.ins");
        File.WriteAllText(baseIns, "base");

        var naming = new ManualNamingStrategy();
        var resolver = new StaticPathResolver(temp.AbsolutePath, naming);
        var config = new SimulationGeneratorConfig(
            parallel: false,
            cpuCount: 1,
            simulations: plannedSimulations.Select(name => new TestSimulation(name)),
            insFiles: [baseIns],
            pfts: [],
            namingStrategy: naming,
            catalog: new TestCatalog(indexedSimulations));

        return new SimulationBatch(resolver, config);
    }

    private static string CreateOutputDirectory(
        TempDirectory temp,
        params string[] path)
    {
        string directory = Path.Combine(path.Prepend(temp.AbsolutePath).ToArray());
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "output.txt"), "old output");
        return directory;
    }
}
