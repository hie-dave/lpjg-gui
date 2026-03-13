using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Entities;
using LpjGuess.Core.Models.Importer;

namespace LpjGuess.Tests.Core.Models;

public class SimpleModelTypesTests
{
    [Fact]
    public void OutputFile_ToString_ReturnsMetadataFileName()
    {
        var metadata = new OutputFileMetadata(
            fileName: "file_lai",
            name: "LAI",
            description: "Leaf Area Index",
            layers: new StaticLayers(["TeBE"], new Unit("m2/m2"), AggregationLevel.Gridcell, TemporalResolution.Daily),
            level: AggregationLevel.Gridcell,
            resolution: TemporalResolution.Daily);

        var file = new OutputFile(metadata, "/tmp/lai.out");

        Assert.Equal("file_lai", file.ToString());
        Assert.Equal("/tmp/lai.out", file.Path);
        Assert.Equal(metadata, file.Metadata);
    }

    [Fact]
    public void OutputFileMetadata_LongNames_IncludeResolutionAndLevel()
    {
        var metadata = new OutputFileMetadata(
            fileName: "file_lai",
            name: "LAI",
            description: "Leaf Area Index",
            layers: new StaticLayers(["TeBE"], new Unit("m2/m2"), AggregationLevel.Patch, TemporalResolution.Annual),
            level: AggregationLevel.Patch,
            resolution: TemporalResolution.Annual);

        Assert.Equal("Annual Patch-Level LAI", metadata.GetLongName());
        Assert.Equal("Annual Patch-Level Leaf Area Index", metadata.GetLongDescription());
    }

    [Fact]
    public void Workspace_DefaultConstructor_InitializesCollections()
    {
        var workspace = new Workspace();

        Assert.Empty(workspace.InstructionFiles);
        Assert.Empty(workspace.Graphs);
        Assert.Empty(workspace.Experiments);
        Assert.Equal(string.Empty, workspace.FilePath);
    }

    [Fact]
    public void Workspace_ForInsFile_ConfiguresBaselineAndFilePath()
    {
        string insFile = Path.Combine("/tmp", "example.ins");

        Workspace workspace = Workspace.ForInsFile(insFile);

        Assert.Single(workspace.InstructionFiles);
        Assert.Equal(insFile, workspace.InstructionFiles[0]);
        Assert.Equal(Path.ChangeExtension(insFile, Workspace.DefaultFileExtension), workspace.FilePath);
        Assert.Single(workspace.Experiments);
        Assert.Equal("Baseline", workspace.Experiments[0].Name);
    }

    [Fact]
    public void Workspace_GetOutputDirectory_UsesWorkspaceDirectory()
    {
        var workspace = new Workspace { FilePath = "/tmp/work/test.lpj" };

        string outputDirectory = workspace.GetOutputDirectory();

        Assert.Equal(Path.Combine("/tmp/work", ".simulations"), outputDirectory);
    }

    [Fact]
    public void Workspace_GetOutputDirectory_UsesRelativePathWhenNoDirectoryComponent()
    {
        var workspace = new Workspace { FilePath = "relative.lpj" };

        string outputDirectory = workspace.GetOutputDirectory();

        Assert.Equal(".simulations", outputDirectory);
    }

    [Fact]
    public void LayerMetadata_StoresConstructorValues()
    {
        var metadata = new LayerMetadata("TeBE", new Unit("m2/m2"));

        Assert.Equal("TeBE", metadata.Name);
        Assert.Equal("m2/m2", metadata.Units.Name);
    }
}
