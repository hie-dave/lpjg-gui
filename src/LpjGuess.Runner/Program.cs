using LpjGuess.Runner.Models;
using LpjGuess.Runner.Parsers;

const string appName = "lpjg-experiment";

if (args.Length == 0)
{
	Console.Error.WriteLine($"Usage: {appName} <config-file>");
	return 1;
}

string inputFile = args[0];
IParser parser = new TomlParser();
RunnerConfiguration input = parser.Parse(inputFile);

CancellationTokenSource cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, args) =>
{
	cancellation.Cancel();

	// Setting args.Cancel to false causes the application to exit when this
	// event handler exits. We need to continue execution in order to kill any
	// potential child processes.
	args.Cancel = true;
};

IEnumerable<Job> jobs = input.GenerateAllJobs(cancellation.Token);
IProgressReporter progress = new ConsoleProgressReporter();

// Don't stream stdout/stderr to this process' tty; it will be included in
// a model exception if one is thrown due to model execution failure.
IOutputHelper outputHelper = new OutputIgnorer();

JobManagerConfiguration jobSettings = input.Settings.ToJobManagerConfig();
JobManager jobManager = new JobManager(jobSettings, progress, outputHelper, jobs);
try
{
	await jobManager.RunAllAsync(cancellation.Token);
	Console.WriteLine();
}
catch (ModelException ex)
{
	Console.Error.WriteLine(ex.Message);
	return 1;
}

return 0;
