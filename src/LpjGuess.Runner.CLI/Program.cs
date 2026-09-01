using System.Text.Json;
using CommandLine;
using LpjGuess.Runner;
using LpjGuess.Runner.CLI;
using LpjGuess.Runner.Models;
using LpjGuess.Runner.Parsers;
using LpjGuess.Runner.Protocol;

string[] parserArgs = NormaliseLegacyArguments(args);
Parser parser = new(settings =>
{
    settings.HelpWriter = Console.Error;
    settings.CaseInsensitiveEnumValues = true;
    // `version` is a real verb because it reports the machine protocol
    // version, rather than the assembly version emitted by AutoVersion.
    settings.AutoVersion = false;
});

ParserResult<object> parsed = parser.ParseArguments<RunOptions, VersionOptions>(parserArgs);
return await parsed.MapResult(
    (RunOptions options) => RunAsync(options),
    (VersionOptions _) => ShowVersionAsync(),
    errors => Task.FromResult(ParserExitCode(errors)));

static async Task<int> RunAsync(RunOptions options)
{
    bool machineMode = false;
    try
    {
        RunInputMode mode = options.Validate();
        machineMode = mode == RunInputMode.Json;
        return mode == RunInputMode.Json
            ? await RunMachineAsync(options.RequestJson!)
            : await RunTomlAsync(options.ConfigFile!);
    }
    catch (OperationCanceledException)
    {
        if (machineMode)
            new NdjsonEventWriter(Console.Out).Write(
                new ProtocolEvent("cancelled", Message: "Run cancelled."));
        else
            Console.Error.WriteLine("Run cancelled.");
        return 130;
    }
    catch (Exception error)
    {
        if (machineMode)
            new NdjsonEventWriter(Console.Out).Write(
                new ProtocolEvent("error", Message: error.ToString()));
        else
            Console.Error.WriteLine(error.Message);
        return 1;
    }
}

static Task<int> ShowVersionAsync()
{
    Console.WriteLine($"lpjg-experiment protocol {ProtocolVersion.Current}");
    return Task.FromResult(0);
}

static async Task<int> RunTomlAsync(string inputFile)
{
    RunnerConfiguration config = new TomlParser().Parse(inputFile);
    using CancellationTokenSource cancellation = CreateCancellationSource();
    ExperimentResult result = await new ExperimentRunner().RunAsync(
        config, resolver: null, new ConsoleProgressReporter(),
        new OutputIgnorer(), config.Settings.CleanPolicy, cancellation.Token);
    Console.WriteLine();
    PrintSummary(result);
    return result.FailedJobs == 0 && result.Error is null ? 0 : 1;
}

static async Task<int> RunMachineAsync(string requestPath)
{
    string json = requestPath == "-"
        ? await Console.In.ReadToEndAsync()
        : await File.ReadAllTextAsync(requestPath);
    RunRequest request = JsonSerializer.Deserialize<RunRequest>(
        json, ProtocolJson.Options)
        ?? throw new InvalidOperationException("The JSON request was empty.");
    (RunnerConfiguration config, var policy) = RunRequestMapper.Map(request);

    NdjsonEventWriter events = new(Console.Out);
    events.Write(new ProtocolEvent("started", new
    {
        protocol_version = ProtocolVersion.Current
    }));

    using CancellationTokenSource cancellation = CreateCancellationSource();
    ExperimentResult result = await new ExperimentRunner().RunAsync(
        config, resolver: null, events, events, policy, cancellation.Token);
    CompletedData completed = new(
        result.TotalJobs, result.SuccessfulJobs, result.FailedJobs, result.Error,
        result.Results.Select(job => new JobResultDto(
            job.Name, job.Duration.TotalSeconds)).ToList());
    events.Write(new ProtocolEvent("completed", completed));
    return result.FailedJobs == 0 && result.Error is null ? 0 : 1;
}

static CancellationTokenSource CreateCancellationSource()
{
    CancellationTokenSource source = new();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        source.Cancel();
    };
    return source;
}

static void PrintSummary(ExperimentResult result)
{
    Console.WriteLine($"Total jobs: {result.TotalJobs}");
    Console.WriteLine($"Successful jobs: {result.SuccessfulJobs}");
    Console.WriteLine($"Failed jobs: {result.FailedJobs}");
    if (result.Error is not null)
        Console.Error.WriteLine(result.Error);
}

static string[] NormaliseLegacyArguments(string[] arguments)
{
    if (arguments.Length == 1 && arguments[0] == "--version")
        return ["version"];
    if (arguments.Length > 0 && !arguments[0].StartsWith('-') &&
        arguments[0] is not ("run" or "version"))
        return ["run", .. arguments];
    return arguments;
}

static int ParserExitCode(IEnumerable<Error> errors)
    => errors.All(error => error is HelpRequestedError or HelpVerbRequestedError)
        ? 0
        : 2;
