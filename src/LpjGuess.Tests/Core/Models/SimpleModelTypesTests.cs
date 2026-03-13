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

        using TempDirectory tmp = TempDirectory.Create();
        string filePath = Path.Combine(tmp.AbsolutePath, "lai.out");
        var file = new OutputFile(metadata, filePath);

        Assert.Equal("file_lai", file.ToString());
        Assert.Equal(filePath, file.Path);
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
        using TempDirectory temp = TempDirectory.Create();
        string insFile = Path.Combine(temp.AbsolutePath, "example.ins");

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
        using TempDirectory temp = TempDirectory.Create();
        string filePath = Path.Combine(temp.AbsolutePath, "work", "test.lpj");
        var workspace = new Workspace { FilePath = filePath };

        string outputDirectory = workspace.GetOutputDirectory();

        string expect = Path.Combine(temp.AbsolutePath, "work", ".simulations");
        Assert.Equal(expect, outputDirectory);
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
