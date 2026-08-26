using System.Text.Json.Serialization;

namespace LpjGuess.Runner.Protocol;

public static class ProtocolVersion
{
    public const int Current = 1;
}

public sealed record RunRequest
{
    public int ProtocolVersion { get; init; } = Protocol.ProtocolVersion.Current;
    public required RunSettingsDto Settings { get; init; }
    public required IReadOnlyList<SimulationDto> Simulations { get; init; }
    public required IReadOnlyList<string> InstructionFiles { get; init; }
    public IReadOnlyList<string> Pfts { get; init; } = [];
    public string? ExistingOutputPolicy { get; init; }
}

public sealed record RunSettingsDto
{
    public bool DryRun { get; init; }
    public bool RunLocal { get; init; } = true;
    public required string OutputDirectory { get; init; }
    public required string GuessPath { get; init; }
    public string InputModule { get; init; } = "nc";
    public ushort CpuCount { get; init; } = 1;
    public string Walltime { get; init; } = "01:00:00";
    public uint Memory { get; init; } = 1;
    public string Queue { get; init; } = "local";
    public string Project { get; init; } = "local";
    public bool EmailNotifications { get; init; }
    public string EmailAddress { get; init; } = "";
    public string JobName { get; init; } = "lpjguess";
    public bool FullFactorial { get; init; } = true;
    public bool UseCpuAffinity { get; init; } = true;
}

public sealed record SimulationDto
{
    public required string Name { get; init; }
    public required IReadOnlyList<FactorDto> Factors { get; init; }
}

public sealed record FactorDto
{
    public required string Type { get; init; }
    public required string Name { get; init; }
    public required string Value { get; init; }
    public string? BlockType { get; init; }
    public string? BlockName { get; init; }
}

public sealed record ProtocolEvent(
    string Type,
    object? Data = null,
    string? Message = null);

public sealed record ProgressData(
    double Percent,
    double ElapsedSeconds,
    int Completed,
    int Total);

public sealed record OutputData(string Job, string Text, string Stream);

public sealed record JobResultDto(string Name, double DurationSeconds);

public sealed record CompletedData(
    int TotalJobs,
    int SuccessfulJobs,
    int FailedJobs,
    string? Error,
    IReadOnlyList<JobResultDto> Results);
