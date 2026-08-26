using CommandLine;

namespace LpjGuess.Runner.CLI;

public enum RunInputMode
{
    Toml,
    Json
}

[Verb("run", HelpText = "Run an LPJ-GUESS experiment.")]
public sealed class RunOptions
{
    [Value(0, MetaName = "config.toml", Required = false,
        HelpText = "TOML experiment configuration.")]
    public string? ConfigFile { get; init; }

    [Option("request-json", MetaValue = "PATH|-",
        HelpText = "Protocol-v1 JSON request path, or '-' for stdin.")]
    public string? RequestJson { get; init; }

    [Option("events", MetaValue = "FORMAT",
        HelpText = "Machine event format. Currently only 'ndjson'.")]
    public string? Events { get; init; }

    public RunInputMode Validate()
    {
        bool hasToml = !string.IsNullOrWhiteSpace(ConfigFile);
        bool hasJson = !string.IsNullOrWhiteSpace(RequestJson);
        if (hasToml == hasJson)
            throw new ArgumentException(
                "Specify exactly one TOML config or --request-json <path|->.");

        if (hasToml)
        {
            if (Events is not null)
                throw new ArgumentException(
                    "--events is only valid with --request-json.");
            return RunInputMode.Toml;
        }

        if (!string.Equals(Events, "ndjson", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(
                "JSON requests require --events ndjson.");
        return RunInputMode.Json;
    }
}

[Verb("version", HelpText = "Show the machine protocol version.")]
public sealed class VersionOptions;
