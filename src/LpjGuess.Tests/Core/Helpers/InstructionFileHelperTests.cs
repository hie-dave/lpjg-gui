using LpjGuess.Core.Helpers;
using LpjGuess.Core.Parsers;
using Microsoft.Extensions.Logging;
using Moq;

namespace LpjGuess.Tests.Core.Helpers;

public class InstructionFileHelperTests
{
    private static InstructionFileHelper CreateHelper(string content, string path)
    {
        var parser = new InstructionFileParser(content, path);
        var logger = new Mock<ILogger<InstructionFileHelper>>();
        return new InstructionFileHelper(parser, logger.Object);
    }

    [Fact]
    public void GetGridlist_UsesTopLevelFileGridlist_WhenPresent()
    {
        var helper = CreateHelper(
            """
            file_gridlist "inputs/grid.txt"
            """,
            "/workspace/experiment/run.ins");

        string path = helper.GetGridlist();

        Assert.Equal(Path.GetFullPath("/workspace/experiment/inputs/grid.txt"), path);
    }

    [Fact]
    public void GetGridlist_FallsBackToParamBlock_WhenTopLevelMissing()
    {
        var helper = CreateHelper(
            """
            param "file_gridlist_cf" (
                str "cf/gridlist.txt"
            )
            """,
            "/workspace/experiment/run.ins");

        string path = helper.GetGridlist();

        Assert.Equal(Path.GetFullPath("/workspace/experiment/cf/gridlist.txt"), path);
    }

    [Fact]
    public void GetGridlist_Throws_WhenMissing()
    {
        var helper = CreateHelper("npatch 1", "/workspace/experiment/run.ins");

        var error = Assert.Throws<InvalidOperationException>(() => helper.GetGridlist());

        Assert.Contains("does not contain a gridlist parameter", error.Message);
    }

    [Fact]
    public void GetOutputDirectory_ReturnsNormalisedPath()
    {
        var helper = CreateHelper(
            """
            outputdirectory "outputs"
            """,
            "/workspace/experiment/run.ins");

        string output = helper.GetOutputDirectory();

        Assert.Equal(Path.GetFullPath("/workspace/experiment/outputs"), output);
    }

    [Fact]
    public void GetOutputDirectory_Throws_WhenMissing()
    {
        var helper = CreateHelper("npatch 1", "/workspace/experiment/run.ins");

        Assert.Throws<ArgumentException>(() => helper.GetOutputDirectory());
    }

    [Fact]
    public void GetEnabledPfts_ReturnsOnlyIncludedPfts()
    {
        var helper = CreateHelper(
            """
            pft "Enabled" (
                include 1
            )
            pft "Disabled" (
                include 0
            )
            pft "MissingInclude" (
                lifeform "tree"
            )
            """,
            "/workspace/experiment/run.ins");

        string[] enabled = helper.GetEnabledPfts().ToArray();

        Assert.Equal(new[] { "Enabled" }, enabled);
    }

    [Fact]
    public void GetNumPatches_ReturnsValue()
    {
        var helper = CreateHelper("npatch 17", "/workspace/experiment/run.ins");

        Assert.Equal(17, helper.GetNumPatches());
    }

    [Fact]
    public void GetNumPatches_Throws_WhenMissing()
    {
        var helper = CreateHelper("outputdirectory \"out\"", "/workspace/experiment/run.ins");

        Assert.Throws<ArgumentException>(() => helper.GetNumPatches());
    }

    [Fact]
    public void GetEnabledStands_RespectsIncludeLandcoverAndRunSwitch()
    {
        var helper = CreateHelper(
            """
            run_urban 1
            run_crop 0

            st "UrbanOn" (
                stinclude 1
                landcover "urban"
            )

            st "UrbanOffByInclude" (
                stinclude 0
                landcover "urban"
            )

            st "CropOffByRunFlag" (
                stinclude 1
                landcover "crop"
            )
            """,
            "/workspace/experiment/run.ins");

        string[] stands = helper.GetEnabledStands().ToArray();

        Assert.Equal(new[] { "UrbanOn" }, stands);
    }

    [Fact]
    public void GetEnabledStands_Throws_ForInvalidLandcover()
    {
        var helper = CreateHelper(
            """
            st "Bad" (
                stinclude 1
                landcover "not_a_landcover"
            )
            """,
            "/workspace/experiment/run.ins");

        Assert.Throws<ArgumentException>(() => helper.GetEnabledStands().ToList());
    }
}
