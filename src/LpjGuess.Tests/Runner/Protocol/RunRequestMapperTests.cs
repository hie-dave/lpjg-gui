using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Runner.Protocol;

namespace LpjGuess.Tests.Runner.Protocol;

public class RunRequestMapperTests
{
    [Fact]
    public void Map_MapsCompleteRequest()
    {
        RunRequest request = CreateRequest() with
        {
            ExistingOutputPolicy = "clean_managed,prune_stale"
        };

        var (configuration, policy) = RunRequestMapper.Map(request);

        Assert.Equal(ExistingOutputPolicy.CleanManaged |
                     ExistingOutputPolicy.PruneStale, policy);
        Assert.Equal("job", configuration.Settings.JobName);
        Assert.Equal((ushort)2, configuration.Settings.CpuCount);
        Assert.Single(configuration.Factors);
        var factors = configuration.Factors[0].Changes.ToList();
        Assert.IsType<TopLevelParameter>(factors[0]);
        BlockParameter block = Assert.IsType<BlockParameter>(factors[1]);
        Assert.Equal("MRS", block.BlockName);
        Assert.Equal("26", block.Value);
    }

    [Fact]
    public void Map_RejectsUnsupportedProtocolVersion()
    {
        RunRequest request = CreateRequest() with { ProtocolVersion = 99 };
        Assert.Throws<ArgumentException>(() => RunRequestMapper.Map(request));
    }

    [Fact]
    public void Map_RejectsIncompleteBlockFactor()
    {
        RunRequest request = CreateRequest() with
        {
            Simulations =
            [
                new SimulationDto
                {
                    Name = "bad",
                    Factors =
                    [
                        new FactorDto
                        {
                            Type = "block", Name = "sla", Value = "26"
                        }
                    ]
                }
            ]
        };
        Assert.Throws<ArgumentException>(() => RunRequestMapper.Map(request));
    }

    private static RunRequest CreateRequest() => new()
    {
        Settings = new RunSettingsDto
        {
            GuessPath = "/guess",
            OutputDirectory = "/output",
            CpuCount = 2,
            JobName = "job"
        },
        InstructionFiles = ["site.ins"],
        Pfts = ["MRS"],
        Simulations =
        [
            new SimulationDto
            {
                Name = "sim",
                Factors =
                [
                    new FactorDto
                    {
                        Type = "top_level", Name = "wateruptake", Value = "wcont"
                    },
                    new FactorDto
                    {
                        Type = "block", BlockType = "pft", BlockName = "MRS",
                        Name = "sla", Value = "26"
                    }
                ]
            }
        ]
    };
}
