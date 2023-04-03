using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Runners;
using LpjGuess.Core.Runners.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace LpjGuess.Core.Runners;

/// <summary>
/// This class will run lpj-guess on the local machine.
/// </summary>
public class LocalRunner : IRunner
{
	/// <summary>
	/// Name of the guess executable.
	/// </summary>
#if WINDOWS
	private const string exeName = "guesscmd.exe";
#else
	private const string exeName = "guess";
#endif

	/// <summary>
	/// Name of the PATH environment variable.
	/// </summary>
	private const string pathVariableName = "PATH";

	/// <summary>
	/// CLI argument which should precede the input module.
	/// </summary>
	private const string inputCliArgument = "-input";

	/// <summary>
	/// The lpj-guess process. This is initialised in the constructor, but the
	/// process itself is not started until Launch() is called.
	/// </summary>
	private readonly Process process;

	/// <summary>
	/// User-specified function which will be called when the child process
	/// writes to stdout.
	/// </summary>
	private readonly Action<string> stdoutCallback;

	/// <summary>
	/// User-specified function which will be called when the child process
	/// writes to stderr.
	/// </summary>
	private readonly Action<string> stderrCallback;

	/// <summary>
	/// User-specified function which will be called when the child process
	/// exits.
	/// </summary>
	private readonly Action<int> completedCallback;

	/// <summary>
	/// Create a new <see cref="LocalRunner"/> instance.
	/// </summary>
	/// <param name="runnerConfig">Configuration settings which describe how to run the model.</param>
	/// <param name="simulation">Settings which describe how to run a particular simulation.</param>
	/// <param name="stdoutCallback">Function to be called when the process writes to stdout.</param>
	/// <param name="stderrCallback">Function to be called when the process writes to stderr.</param>
	/// <param name="onCompleted">Function to be called when the process exits. The function argument is the exit code of the process.</param>
	public LocalRunner(LocalRunnerConfiguration runnerConfig, ISimulation simulation,
		Action<string> stdoutCallback, Action<string> stderrCallback, Action<int> onCompleted)
	{
		// Initialise the process but don't start it.
		process = new Process();
		process.StartInfo.FileName = runnerConfig.GuessPath;
		process.StartInfo.ArgumentList.Add(inputCliArgument);
		process.StartInfo.ArgumentList.Add(simulation.InputModule);
		process.StartInfo.ArgumentList.Add(simulation.InsFile);
		process.StartInfo.WorkingDirectory = Path.GetDirectoryName(simulation.InsFile);
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.EnableRaisingEvents = true;
		process.OutputDataReceived += OnOutputDataReceived;
		process.ErrorDataReceived += OnErrorDataReceived;
		process.Exited += OnCompleted;

		this.stdoutCallback = stdoutCallback;
		this.stderrCallback = stderrCallback;
		this.completedCallback = onCompleted;
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		process.OutputDataReceived -= OnOutputDataReceived;
		process.ErrorDataReceived -= OnErrorDataReceived;
		process.Exited -= OnCompleted;
		process.Dispose();
	}

	/// <summary>
	/// Launch the guess process (non-blocking).
	/// </summary>
	public void Run()
	{
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
	}

	/// <summary>
	/// True iff the guess process is currently running.
	/// </summary>
	public bool IsRunning => !process.HasExited;

	/// <summary>
	/// The exit code of the process.
	/// </summary>
	public int ExitCode => process.ExitCode;

	/// <summary>
	/// Stop the running guess proces.
	/// </summary>
	public void Cancel()
	{
		process.Kill();
	}

	/// <summary>
	/// Search for the specified command on PATH. Return null if not found.
	/// </summary>
	/// <param name="fileName">Command/executable to search for.</param>
	private string? FindOnPath(string fileName)
	{
		// PATH delimiter character.
#if WINDOWS
		const string delim = ";";
#else
		const string delim = ":";
#endif

		string? path = Environment.GetEnvironmentVariable(pathVariableName);
		if (path == null)
			return null;

		foreach (string directory in path.Split(delim))
		{
			string resolved = Path.Combine(directory, fileName);
			if (File.Exists(resolved))
				return resolved;
		}

		return null;
	}

	/// <summary>
	/// Get the path to the lpj-guess executable to be used for the run.
	/// </summary>
	private string GetGuessPath()
	{
		// // Attempt to use the user-specified override, if one is set.
		// if (!string.IsNullOrWhiteSpace(Configuration.Instance.CustomExecutable))
		// {
		// 	if (!File.Exists(Configuration.Instance.CustomExecutable))
		// 		throw new Exception($"Custom executable is set to '{Configuration.Instance.CustomExecutable}', but this file does not exist.");
		// 	return Configuration.Instance.CustomExecutable;
		// }

		// Look in the same directory as this program.
		string assembly = Assembly.GetExecutingAssembly().Location;
		string? cwd = Path.GetDirectoryName(assembly);
		if (cwd != null)
		{
			string local = Path.Combine(cwd, exeName);
			if (File.Exists(local))
				return local;
		}

		// Search on PATH.
		string? systemGuess = FindOnPath(exeName);
		if (systemGuess != null)
			return systemGuess;

		// Out of options - time to bail out.
		throw GuessNotFoundException();
	}

	/// <summary>
	/// Return an exception suitable for the scenario in which the guess
	/// executable cannot be located.
	/// </summary>
	private static Exception GuessNotFoundException()
	{
		return new Exception($"Unable to locate guess executable. Consider adding it to PATH or setting a custom path in preferences."); 
	}

	/// <summary>
	/// Called when the child process exits.
	/// </summary>
	/// <param name="sender">Sender object (the process).</param>
	/// <param name="e">Event data.</param>
	private void OnCompleted(object? sender, EventArgs e)
	{
		completedCallback(process.ExitCode);
	}

	/// <summary>
	/// Called when the child process writes to stderr.
	/// </summary>
	/// <param name="sender">Sender object (the process).</param>
	/// <param name="e">Event data.</param>
	private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
	{
		if (!string.IsNullOrEmpty(e.Data))
			stderrCallback(e.Data);
	}

	/// <summary>
	/// Called when the child process writes to stdout.
	/// </summary>
	/// <param name="sender">Sender object (the process).</param>
	/// <param name="e">Event data.</param>
	private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		if (!string.IsNullOrEmpty(e.Data))
			stdoutCallback(e.Data);
	}
}
