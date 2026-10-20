using LpjGuess.Runner.CLI;

namespace LpjGuess.Tests.Runner;

public class CliOptionsTests
{
    [Fact]
    public void Validate_AcceptsTomlInput()
    {
        RunOptions options = new() { ConfigFile = "experiment.toml" };
        Assert.Equal(RunInputMode.Toml, options.Validate());
    }

    [Fact]
    public void Validate_AcceptsJsonAndNdjsonEvents()
    {
        RunOptions options = new()
        {
            RequestJson = "-",
            Events = "ndjson"
        };
        Assert.Equal(RunInputMode.Json, options.Validate());
    }

    [Fact]
    public void Validate_RejectsAmbiguousInput()
    {
        RunOptions options = new()
        {
            ConfigFile = "experiment.toml",
            RequestJson = "request.json",
            Events = "ndjson"
        };
        Assert.Throws<ArgumentException>(() => options.Validate());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("xml")]
    public void Validate_RequiresNdjsonForJsonInput(string? events)
    {
        RunOptions options = new()
        {
            RequestJson = "request.json",
            Events = events
        };
        Assert.Throws<ArgumentException>(() => options.Validate());
    }

    [Fact]
    public void NormaliseValidate_AcceptsInputAndOutputFiles()
    {
        NormaliseOptions options = new()
        {
            InputFile = "input.ins",
            OutputFile = "output.ins"
        };

        options.Validate();
    }

    [Theory]
    [InlineData("", "output.ins")]
    [InlineData("input.ins", "")]
    public void NormaliseValidate_RejectsMissingPaths(string input, string output)
    {
        NormaliseOptions options = new()
        {
            InputFile = input,
            OutputFile = output
        };

        Assert.Throws<ArgumentException>(() => options.Validate());
    }
}
