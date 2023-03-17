using LpjGuess.Core.Interfaces.Runners;
using LpjGuess.Core.Models;
using LpjGuess.Core.Runners;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a view which displays an instruction file. This presenter
/// handles logic for running the file or aborting an ongoing run.
/// </summary>
public class FilePresenter : IPresenter<IFileView>
{
	/// <summary>
	/// The instruction file for which this presenter is responsible.
	/// </summary>
	private readonly string file;

	/// <summary>
	/// The view object.
	/// </summary>
	private readonly IFileView view;

	/// <summary>
	/// The runner object. fixme - let's make this non-nullable and reuse it?
	/// </summary>
	private IRunner? runner;

	/// <summary>
	/// Create a new <see cref="FilePresenter"/> instance for the given file.
	/// </summary>
	/// <param name="file">The instruction file.</param>
	public FilePresenter(string file)
	{
		this.file = file;
		view = new FileView(file, OnRun, OnStop, OnConfigureRunners);
	}

	/// <summary>
	/// Called when the user wants to configure the available runners.
	/// </summary>
	private void OnConfigureRunners()
	{
		try
		{
			
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		view.Dispose();
	}

	/// <inheritdoc />
	public IFileView GetView() => view;

	/// <summary>
	/// User has clicked the 'run' button.
	/// </summary>
	private void OnRun()
	{
		try
		{
			if (runner != null && runner.IsRunning)
				throw new InvalidOperationException($"Simulation is already running. Please kill the previous process first.");

			// Clear output buffer from any previous runs.
			view.ClearOutput();

			// Dispose of the previous runner object.
			if (runner != null)
				runner.Dispose();

			// Create a new runner object, and start running the simulation.
			IRunnerConfiguration? runConfig = Configuration.Instance.DefaultRunner;
			if (runConfig == null)
				throw new InvalidOperationException($"No default runner exists.");

			var simulation = new SimulationConfiguration(file, view.InputModule);
			runner = RunnerFactory.Create(runConfig, simulation, StdoutCallback, StderrCallback, OnCompleted);
			runner.Run();

			// Ensure that the stop button is visible and the run button hidden.
			view.ShowRunButton(false);
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to run file '{file}'", error);
		}
	}

	/// <summary>
	/// User has clicked the 'stop' button.
	/// </summary>
	private void OnStop()
	{
		try
		{
			if (runner == null || !runner.IsRunning)
				return;

			runner.Cancel();
			view.ShowRunButton(true);
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to abort execution of file '{file}'", error);
		}
	}

	/// <summary>
	/// The guess process has completed.
	/// </summary>
	private void OnCompleted(int exitCode)
	{
		try
		{
			if (runner != null && !runner.IsRunning)
				view.AppendOutput($"Process exited with code {exitCode}");
			view.ShowRunButton(true);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Called whenever a guess process writes to stdout. Propagates the message
	/// to the user.
	/// </summary>
	/// <param name="stdout">The message written by guess to stdout.</param>
	private void StdoutCallback(string stdout)
	{
		view.AppendOutput(stdout);
	}

	/// <summary>
	/// Called whenever a guess process writes to stderr. Propagates the message
	/// to the user.
	/// </summary>
	/// <param name="stderr">The message written by guess to stderr.</param>
	private void StderrCallback(string stderr)
	{
		view.AppendError(stderr);
	}
}
