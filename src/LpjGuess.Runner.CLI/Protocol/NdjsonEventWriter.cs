using System.Text.Json;
using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Protocol;

public sealed class NdjsonEventWriter : IProgressReporter, IOutputHelper
{
    private readonly TextWriter writer;
    private readonly object sync = new();

    public NdjsonEventWriter(TextWriter writer) => this.writer = writer;

    public void ReportProgress(double percent, TimeSpan elapsed,
                               int ncomplete, int njob)
        => Write(new ProtocolEvent("progress",
            new ProgressData(percent, elapsed.TotalSeconds, ncomplete, njob)));

    public void ReportOutput(string jobName, string output)
        => Write(new ProtocolEvent("output",
            new OutputData(jobName, output, "stdout")));

    public void ReportError(string jobName, string output)
        => Write(new ProtocolEvent("output",
            new OutputData(jobName, output, "stderr")));

    public void Write(ProtocolEvent message)
    {
        string json = JsonSerializer.Serialize(message, ProtocolJson.Options);
        lock (sync)
        {
            writer.WriteLine(json);
            writer.Flush();
        }
    }
}
