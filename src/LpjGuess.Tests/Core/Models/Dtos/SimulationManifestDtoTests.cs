using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Dtos;
using LpjGuess.Core.Models.Factorial.Factors;

namespace LpjGuess.Tests.Core.Models.Dtos;

public class SimulationManifestDtoTests
{
    private static IReadOnlyList<string> Pfts => new[] { "pine", "oak" };

    private static IReadOnlyList<IFactor> Factors => new IFactor[]
    {
        new TopLevelParameter("spinupyears", "1000"),
        BlockParameter.Pft("pine", "sla", "3.4")
    };

    [Fact]
    public void RoundTrip_Preserves_All_Scalar_Fields()
    {
        var manifest = new SimulationManifest(
            Key: "sim-abc123",
            Name: "my-sim",
            Path: "/tmp/out/ins1/sim-abc123",
            BaseIns: "ins1",
            InsFile: "/tmp/out/ins1/sim-abc123/input.ins",
            Pfts: Pfts,
            Factors: Factors,
            GeneratedAtUtc: new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc)
        );

        var dto = SimulationManifestDto.FromSimulationManifest(manifest);
        var roundTrip = dto.ToSimulationManifest();

        Assert.Equal(manifest.Key, roundTrip.Key);
        Assert.Equal(manifest.Name, roundTrip.Name);
        Assert.Equal(manifest.Path, roundTrip.Path);
        Assert.Equal(manifest.BaseIns, roundTrip.BaseIns);
        Assert.Equal(manifest.InsFile, roundTrip.InsFile);
        Assert.Equal(manifest.GeneratedAtUtc, roundTrip.GeneratedAtUtc);
    }

    [Fact]
    public void RoundTrip_Preserves_Pfts_And_Factors()
    {
        var manifest = new SimulationManifest(
            Key: "sim-xyz789",
            Name: "another-sim",
            Path: "/tmp/out/ins2/sim-xyz789",
            BaseIns: "ins2",
            InsFile: "/tmp/out/ins2/sim-xyz789/input.ins",
            Pfts: Pfts,
            Factors: Factors,
            GeneratedAtUtc: DateTime.UtcNow
        );

        var dto = SimulationManifestDto.FromSimulationManifest(manifest);
        var roundTrip = dto.ToSimulationManifest();

        Assert.Equal(Pfts.Count, roundTrip.Pfts.Count);
        Assert.True(Pfts.SequenceEqual(roundTrip.Pfts));

        Assert.Equal(Factors.Count, roundTrip.Factors.Count);
        Assert.IsType<TopLevelParameter>(roundTrip.Factors[0]);
        Assert.IsType<BlockParameter>(roundTrip.Factors[1]);

        var tl = (TopLevelParameter)roundTrip.Factors[0];
        Assert.Equal("spinupyears", tl.Name);
        Assert.Equal("1000", tl.Value);

        var bp = (BlockParameter)roundTrip.Factors[1];
        Assert.Equal("pft", bp.BlockType);
        Assert.Equal("pine", bp.BlockName);
        Assert.Equal("sla", bp.Name);
        Assert.Equal("3.4", bp.Value);
    }
}
