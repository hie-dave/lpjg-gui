using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Runners;
using LpjGuess.Core.Runners.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace LpjGuess.Core.Runners;

using static RunnerConstants;

/// <summary>
/// This class will run lpj-guess on the local machine.
/// </summary>
public class LocalRunner : IRunner
{
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
		string guessPath = runnerConfig.GuessPath;

		// Search on PATH if no executable path provided.
		if (string.IsNullOrEmpty(guessPath))
			guessPath = GetGuessPath();

		// Initialise the process but don't start it.
		process = new Process();
		process.StartInfo.FileName = runnerConfig.GuessPath;
		process.StartInfo.ArgumentList.Add(InputCliArgument);
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
	public bool IsRunning => InterrogateProcess(p => !p.HasExited, false);

	/// <summary>
	/// The exit code of the process.
	/// </summary>
	public int ExitCode => InterrogateProcess(p => p.ExitCode, 1);

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
	private static string? FindOnPath(string fileName)
	{
		// PATH delimiter character.
		string? path = Environment.GetEnvironmentVariable(PathVariableName);
		if (path == null)
			return null;

		foreach (string directory in path.Split(PathDelim))
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
		// Look in the same directory as this program.
		string assembly = Assembly.GetExecutingAssembly().Location;
		string? cwd = Path.GetDirectoryName(assembly);
		if (cwd != null)
		{
			string local = Path.Combine(cwd, ExeName);
			if (File.Exists(local))
				return local;
		}

		// Search on PATH.
		string? systemGuess = FindOnPath(ExeName);
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

	/// <summary>
	/// Attempt to access a member of a process, and return a default value if
	/// the process cannot be interrogated (e.g. if an exception is thrown).
	/// 
	/// This function will never throw.
	/// </summary>
	/// <param name="func">A function which attempts to access the process object.</param>
	/// <param name="default">Default return value if the process cannot be interrogated.</param>
	/// <typeparam name="T">The return type parameter.</typeparam>
	private T InterrogateProcess<T>(Func<Process, T> func, T @default)
	{
		try
		{
			return func(process);
		}
		catch (Exception error)
		{
			Console.Error.WriteLine(error.ToString());
			return @default;
		}
	}
}
