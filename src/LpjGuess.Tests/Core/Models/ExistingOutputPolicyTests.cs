using LpjGuess.Core.Models;

namespace LpjGuess.Tests.Core.Models;

public class ExistingOutputPolicyTests
{
    [Theory]
    [InlineData("preserve", ExistingOutputPolicy.Preserve)]
    [InlineData("prune_stale", ExistingOutputPolicy.PruneStale)]
    [InlineData("clean_managed", ExistingOutputPolicy.CleanManaged)]
    [InlineData("fail", ExistingOutputPolicy.Fail)]
    [InlineData("clean_managed,prune_stale", ExistingOutputPolicy.CleanManaged | ExistingOutputPolicy.PruneStale)]
    [InlineData("clean_managed, prune_stale", ExistingOutputPolicy.CleanManaged | ExistingOutputPolicy.PruneStale)]
    public void ParseExistingOutputPolicy_Parses_Single_And_Comma_Delimited_Flags(
        string value,
        ExistingOutputPolicy expected)
    {
        ExistingOutputPolicy actual =
            ExistingOutputPolicyExtensions.ParseExistingOutputPolicy(value);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseExistingOutputPolicy_Throws_For_Invalid_Value()
    {
        Assert.Throws<ArgumentException>(() =>
            ExistingOutputPolicyExtensions.ParseExistingOutputPolicy("delete_everything"));
    }

    [Theory]
    [InlineData(ExistingOutputPolicy.Preserve, "preserve")]
    [InlineData(ExistingOutputPolicy.CleanManaged, "clean_managed")]
    [InlineData(ExistingOutputPolicy.PruneStale, "prune_stale")]
    [InlineData(ExistingOutputPolicy.CleanManaged | ExistingOutputPolicy.PruneStale, "prune_stale,clean_managed")]
    public void ToConfigString_Serialises_Policy(
        ExistingOutputPolicy policy,
        string expected)
    {
        Assert.Equal(expected, policy.ToConfigString());
    }
}
