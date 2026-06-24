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
            BaseIns: "base",
            InsFile: "input.ins",
            Pfts: [],
            Factors: [],
            GeneratedAtUtc: DateTime.UtcNow);

        var ex = Assert.Throws<AggregateException>(() =>
            catalog.WriteSimulation("/non/existent/path", manifest));
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
            BaseIns: Path.Combine(temp.AbsolutePath, "ins1"),
            InsFile: "input.ins",
            Pfts: ["oak"],
            Factors: factors,
            GeneratedAtUtc: new DateTime(2024, 5, 1, 13, 15, 0, DateTimeKind.Utc));

        catalog.WriteSimulation(simDir.AbsolutePath, manifest);
        var loaded = catalog.ReadManifest(simDir.AbsolutePath);

        Assert.Equal(manifest.Key, loaded.Key);
        Assert.Equal(manifest.Name, loaded.Name);
        Assert.Equal(manifest.BaseIns, loaded.BaseIns);
        Assert.Equal(manifest.InsFile, loaded.InsFile);
        Assert.True(manifest.Pfts.SequenceEqual(loaded.Pfts));
        Assert.Equal(manifest.Factors.Count, loaded.Factors.Count);
        Assert.IsType<TopLevelParameter>(loaded.Factors[0]);
        Assert.IsType<BlockParameter>(loaded.Factors[1]);
        Assert.Equal(manifest.GeneratedAtUtc, loaded.GeneratedAtUtc);
    }

    [Fact]
    public void WriteSimulation_Writes_Portable_Manifest_Paths()
    {
        using TempDirectory temp = TempDirectory.Create();
        using TempDirectory simDir = TempDirectory.Relative(temp, "ins1", "sim-abc");
        string baseIns = Path.Combine(temp.AbsolutePath, "base.ins");
        var catalog = new ResultCatalog();
        var manifest = new SimulationManifest(
            Key: "sim-abc",
            Name: "Name A",
            BaseIns: baseIns,
            InsFile: "generated.ins",
            Pfts: [],
            Factors: [],
            GeneratedAtUtc: new DateTime(2024, 5, 1, 13, 15, 0, DateTimeKind.Utc));

        catalog.WriteSimulation(simDir.AbsolutePath, manifest);

        string content = File.ReadAllText(
            Path.Combine(simDir.AbsolutePath, "manifest.toml"));
        SimulationManifest loaded = catalog.ReadManifest(simDir.AbsolutePath);

        // Paths in the manifest toml will have backslashes encoded with \\.
        // Therefore we can't compare paths to the raw toml string on windows.
        Assert.Equal(baseIns, loaded.BaseIns);
        Assert.Contains("base_ins =", content);
        Assert.Contains("ins_file = \"generated.ins\"", content);
        Assert.DoesNotContain("path =", content);
        Assert.DoesNotContain(simDir.AbsolutePath, content);
    }

    [Fact]
    public void ReadManifest_Accepts_Legacy_Path_Key()
    {
        using TempDirectory simDir = TempDirectory.Create();
        string content = """
            key = "sim-abc"
            name = "Name A"
            path = "/old/non-portable/location"
            base_ins = "/source/base.ins"
            ins_file = "generated.ins"
            generated_at_utc = 2024-05-01T13:15:00Z
            pfts = []
            factors = []
            """;
        File.WriteAllText(Path.Combine(simDir.AbsolutePath, "manifest.toml"), content);

        SimulationManifest loaded = new ResultCatalog().ReadManifest(simDir.AbsolutePath);

        Assert.Equal("sim-abc", loaded.Key);
        Assert.Equal("generated.ins", loaded.InsFile);
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
            BaseIns: Path.Combine(temp.AbsolutePath, "insA"),
            InsFile: "input.ins",
            Pfts: ["pine"],
            Factors: [new TopLevelParameter("param", "1")],
            GeneratedAtUtc: DateTime.UtcNow);

        var m2 = new SimulationManifest(
            Key: "sim-2",
            Name: "Name2",
            BaseIns: Path.Combine(temp.AbsolutePath, "insB"),
            InsFile: "input.ins",
            Pfts: ["oak"],
            Factors: [new TopLevelParameter("param", "2")],
            GeneratedAtUtc: DateTime.UtcNow);

        // Ensure manifests are written (and directories exist)
        catalog.WriteSimulation(sim1Dir.AbsolutePath, m1);
        catalog.WriteSimulation(sim2Dir.AbsolutePath, m2);

        SimulationIndex index = new SimulationIndex([
            "insA/sim-1",
            "insB/sim-2"
        ]);
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
