using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Services;

namespace LpjGuess.Tests.Runner.Services;

public class SimulationServiceTests
{
    private sealed class TestSimulation : ISimulation
    {
        public string Name => "sim-1";
        public IEnumerable<IFactor> Changes => [];

        public void Generate(
            string insFile,
            string targetFile,
            IEnumerable<string> pfts)
        {
            File.WriteAllText(targetFile, "generated");
        }
    }

    private sealed class CapturingCatalog : IResultCatalog
    {
        public string? SimulationDirectory { get; private set; }
        public SimulationManifest? Manifest { get; private set; }
        public SimulationIndex? Index { get; private set; }

        public void WriteIndex(IPathResolver pathResolver, SimulationIndex index)
        {
            Index = index;
        }

        public void WriteSimulation(
            string simulationDirectory,
            SimulationManifest manifest)
        {
            SimulationDirectory = simulationDirectory;
            Manifest = manifest;
        }

        public SimulationManifest ReadManifest(string simulationDirectory)
        {
            throw new NotSupportedException();
        }

        public SimulationIndex ReadIndex(IPathResolver pathResolver)
        {
            throw new NotSupportedException();
        }
    }

    [Fact]
    public void GenerateAllJobs_Creates_Portable_Manifest_With_Utc_Timestamp()
    {
        using TempDirectory temp = TempDirectory.Create();
        string outputDirectory = Path.Combine(temp.AbsolutePath, "out");
        string baseIns = Path.Combine(temp.AbsolutePath, "base.ins");
        File.WriteAllText(baseIns, "base");

        var naming = new ManualNamingStrategy();
        var catalog = new CapturingCatalog();
        var resolver = new StaticPathResolver(outputDirectory, naming);
        var config = new SimulationGeneratorConfig(
            parallel: false,
            cpuCount: 1,
            simulations: [new TestSimulation()],
            insFiles: [baseIns],
            pfts: [],
            namingStrategy: naming,
            catalog: catalog);
        var service = new SimulationService(resolver, config);
        DateTime before = DateTime.UtcNow;

        Job job = Assert.Single(service.GenerateAllJobs(CancellationToken.None));

        DateTime after = DateTime.UtcNow;
        SimulationManifest manifest = Assert.IsType<SimulationManifest>(catalog.Manifest);
        string simulationDirectory = Assert.IsType<string>(catalog.SimulationDirectory);
        Assert.Equal(baseIns, manifest.BaseIns);
        Assert.False(Path.IsPathRooted(manifest.InsFile));
        Assert.Equal(Path.GetFileName(job.InsFile), manifest.InsFile);
        Assert.Equal(
            Path.GetFullPath(job.InsFile),
            Path.GetFullPath(manifest.InsFile, simulationDirectory));
        Assert.Equal(DateTimeKind.Utc, manifest.GeneratedAtUtc.Kind);
        Assert.InRange(manifest.GeneratedAtUtc, before, after);
        Assert.Equal(
            Path.Combine("base", "sim-1"),
            Assert.Single(catalog.Index!.Simulations));
    }
}
