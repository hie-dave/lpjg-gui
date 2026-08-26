using System.Text.Json;
using LpjGuess.Runner.Protocol;

namespace LpjGuess.Tests.Runner.Protocol;

public class NdjsonEventWriterTests
{
    [Fact]
    public void Reporter_WritesOneSnakeCaseJsonObjectPerEvent()
    {
        StringWriter output = new();
        NdjsonEventWriter writer = new(output);

        writer.ReportProgress(50, TimeSpan.FromSeconds(2.5), 1, 2);
        writer.ReportError("job-1", "problem");

        string[] lines = output.ToString().Split(
            Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        using JsonDocument progress = JsonDocument.Parse(lines[0]);
        Assert.Equal("progress", progress.RootElement.GetProperty("type").GetString());
        Assert.Equal(2.5, progress.RootElement.GetProperty("data")
            .GetProperty("elapsed_seconds").GetDouble());
        using JsonDocument error = JsonDocument.Parse(lines[1]);
        Assert.Equal("stderr", error.RootElement.GetProperty("data")
            .GetProperty("stream").GetString());
    }
}
