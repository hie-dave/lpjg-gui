using LpjGuess.Core.Models;
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
}