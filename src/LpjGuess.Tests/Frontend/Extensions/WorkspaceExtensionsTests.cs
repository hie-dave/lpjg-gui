using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Extensions;

namespace LpjGuess.Tests.Frontend.Extensions;

public class WorkspaceExtensionsTests
{
    [Fact]
    public void LoadWorkspace_SetsFilePath_WhenFilePathIsNotSerialised()
    {
        using TempDirectory temp = TempDirectory.Create();
        string workspaceFile = Path.Combine(temp.AbsolutePath, "workspace.lpj");

        Workspace workspace = new()
        {
            FilePath = workspaceFile
        };
        workspace.InstructionFiles.Add("example.ins");
        workspace.Save();

        string json = File.ReadAllText(workspaceFile);
        Assert.DoesNotContain("\"FilePath\"", json);

        Workspace loaded = workspaceFile.LoadWorkspace();

        Assert.Equal(workspaceFile, loaded.FilePath);
        Assert.Single(loaded.InstructionFiles);
        Assert.Equal("example.ins", loaded.InstructionFiles[0]);
    }

    [Fact]
    public void WorkspaceRoundTrip_PreservesNamedScenarios()
    {
        using TempDirectory temp = TempDirectory.Create();
        string workspaceFile = Path.Combine(temp.AbsolutePath, "workspace.lpj");
        var scenario = new CompositeFactor([
            new TopLevelParameter("co2", "550")
        ]) { Name = "Moderate" };
        var experiment = new Experiment(
            "Climate",
            string.Empty,
            string.Empty,
            [],
            [],
            new FactorialGenerator(false, [
                new SimpleFactorGenerator("Scenarios", [scenario])
            ]));
        var workspace = new Workspace
        {
            FilePath = workspaceFile,
            Experiments = [experiment]
        };

        workspace.Save();
        Workspace loaded = workspaceFile.LoadWorkspace();

        FactorialGenerator generator = Assert.IsType<FactorialGenerator>(
            loaded.Experiments.Single().SimulationGenerator);
        SimpleFactorGenerator scenarios = Assert.IsType<SimpleFactorGenerator>(
            generator.Factors.Single());
        CompositeFactor loadedScenario = Assert.IsType<CompositeFactor>(
            scenarios.Levels.Single());
        Assert.Equal("Moderate", loadedScenario.Name);
    }

    [Fact]
    public void WorkspaceRoundTrip_PreservesExistingOutputPolicy()
    {
        using TempDirectory temp = TempDirectory.Create();
        string workspaceFile = Path.Combine(temp.AbsolutePath, "workspace.lpj");
        var workspace = new Workspace
        {
            FilePath = workspaceFile,
            ExistingOutputPolicy = ExistingOutputPolicy.CleanManaged |
                                   ExistingOutputPolicy.PruneStale
        };

        workspace.Save();

        string json = File.ReadAllText(workspaceFile);
        Assert.Contains("\"ExistingOutputPolicy\"", json);
        Assert.Contains("clean_managed", json);
        Assert.Contains("prune_stale", json);

        Workspace loaded = workspaceFile.LoadWorkspace();

        Assert.Equal(workspace.ExistingOutputPolicy, loaded.ExistingOutputPolicy);
    }
}
