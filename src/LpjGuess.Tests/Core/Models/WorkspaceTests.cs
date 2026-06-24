using LpjGuess.Core.Models;

namespace LpjGuess.Tests.Core.Models;

public class WorkspaceTests
{
    [Fact]
    public void Default_ExistingOutputPolicy_Is_Preserve()
    {
        Workspace workspace = new Workspace();

        Assert.Equal(ExistingOutputPolicy.Preserve, workspace.ExistingOutputPolicy);
    }
}
