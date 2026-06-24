using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Factorial;

namespace LpjGuess.Tests.Core.Models.Factorial;

public class ExperimentTests
{
    [Fact]
    public void DefaultConstructor_Uses_Default_Run_Settings()
    {
        Experiment experiment = new Experiment();

        Assert.Equal("nc", experiment.InputModule);
        Assert.Equal(
            ExistingOutputPolicy.CleanManaged,
            experiment.ExistingOutputPolicy);
    }

    [Fact]
    public void CreateBaseline_Uses_Default_Run_Settings()
    {
        Experiment experiment = Experiment.CreateBaseline();

        Assert.Equal("nc", experiment.InputModule);
        Assert.Equal(
            ExistingOutputPolicy.CleanManaged,
            experiment.ExistingOutputPolicy);
    }
}
