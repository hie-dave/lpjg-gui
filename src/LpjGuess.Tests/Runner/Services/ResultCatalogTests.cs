using System.Threading.Tasks;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Services;

namespace LpjGuess.Tests.Runner.Services;

public class ResultCatalogTests
{
    [Fact]
    public void WriteSimulation_Throws_When_Directory_Missing()
    {
        var catalog = new ResultCatalog();
        var manifest = new SimulationManifest(
            Key: "sim-1",
            Name: "name",
            Path: "/non/existent/path",
            BaseIns: "base",
            InsFile: "input.ins",
            Pfts: [],
            Factors: [],
            GeneratedAtUtc: DateTime.UtcNow);

        var ex = Assert.Throws<AggregateException>(() => catalog.WriteSimulation(manifest));
        Assert.IsType<DirectoryNotFoundException>(ex.InnerException);
    }

    [Fact]
    public void Write_And_Read_Manifest_RoundTrip()
    {
        using var temp = TempDirectory.Create();
        var simDir = TempDirectory.Relative(temp, "ins1", "sim-abc");
        var catalog = new ResultCatalog();

        var factors = new IFactor[]
        {
            new TopLevelParameter("paramA", "42"),
            BlockParameter.Pft("oak", "sla", "3.1")
        };

        var manifest = new SimulationManifest(
            Key: "sim-abc",
            Name: "Name A",
            Path: simDir.AbsolutePath,
            BaseIns: "ins1",
            InsFile: Path.Combine(simDir.AbsolutePath, "input.ins"),
            Pfts: ["oak"],
            Factors: factors,
            GeneratedAtUtc: new DateTime(2024, 5, 1, 13, 15, 0, DateTimeKind.Utc));

        catalog.WriteSimulation(manifest);
        var loaded = catalog.ReadManifest(simDir.AbsolutePath);

        Assert.Equal(manifest.Key, loaded.Key);
        Assert.Equal(manifest.Name, loaded.Name);
        Assert.Equal(manifest.Path, loaded.Path);
        Assert.Equal(manifest.BaseIns, loaded.BaseIns);
        Assert.Equal(manifest.InsFile, loaded.InsFile);
        Assert.True(manifest.Pfts.SequenceEqual(loaded.Pfts));
        Assert.Equal(manifest.Factors.Count, loaded.Factors.Count);
        Assert.IsType<TopLevelParameter>(loaded.Factors[0]);
        Assert.IsType<BlockParameter>(loaded.Factors[1]);
        Assert.Equal(manifest.GeneratedAtUtc, loaded.GeneratedAtUtc);
    }

    [Fact]
    public void Write_And_Read_Index_RoundTrip()
    {
        using var temp = TempDirectory.Create();
        IPathResolver resolver = CreatePathResolver(temp);

        var sim1Dir = TempDirectory.Relative(temp, "insA", "sim-1");
        var sim2Dir = TempDirectory.Relative(temp, "insB", "sim-2");

        var catalog = new ResultCatalog();

        var m1 = new SimulationManifest(
            Key: "sim-1",
            Name: "Name1",
            Path: sim1Dir.AbsolutePath,
            BaseIns: "insA",
            InsFile: Path.Combine(sim1Dir.AbsolutePath, "input.ins"),
            Pfts: ["pine"],
            Factors: [new TopLevelParameter("param", "1")],
            GeneratedAtUtc: DateTime.UtcNow);

        var m2 = new SimulationManifest(
            Key: "sim-2",
            Name: "Name2",
            Path: sim2Dir.AbsolutePath,
            BaseIns: "insB",
            InsFile: Path.Combine(sim2Dir.AbsolutePath, "input.ins"),
            Pfts: ["oak"],
            Factors: [new TopLevelParameter("param", "2")],
            GeneratedAtUtc: DateTime.UtcNow);

        // Ensure manifests are written (and directories exist)
        catalog.WriteSimulation(m1);
        catalog.WriteSimulation(m2);

        SimulationIndex index = new SimulationIndex([m1.Path, m2.Path]);
        catalog.WriteIndex(resolver, index);

        SimulationIndex loaded = catalog.ReadIndex(resolver);

        Assert.Equal(index.Simulations, loaded.Simulations);
    }

    [Fact]
    public async Task ReadManifest_Throws_When_Not_Found()
    {
        using TempDirectory temp = TempDirectory.Create();
        ResultCatalog catalog = new ResultCatalog();
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await catalog.ReadManifestAsync(temp.AbsolutePath));
    }

    [Fact]
    public async Task ReadIndex_Throws_When_Not_Found()
    {
        using TempDirectory temp = TempDirectory.Create();
        IPathResolver resolver = CreatePathResolver(temp);
        ResultCatalog catalog = new ResultCatalog();
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await catalog.ReadIndexAsync(resolver));
    }

    private IPathResolver CreatePathResolver(TempDirectory temp)
    {
        ISimulationNamingStrategy namingStrategy = new ManualNamingStrategy();
        return new StaticPathResolver(temp.AbsolutePath, namingStrategy);
    }
}
