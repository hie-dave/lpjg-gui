namespace LpjGuess.Runner.Models;

/// <summary>
/// Interface for running LPJ-Guess simulations.
/// </summary>
public interface IRunner
{
	/// <summary>
	/// Run the simulation.
	/// </summary>
	/// <param name="job">The job to be run.</param>
	/// <param name="ct">Cancellation token.</param>
	Task RunAsync(Job job, CancellationToken ct);
}

/// <summary>
/// Interface to a runner which can be monitored.
/// </summary>
public interface IMonitorableRunner : IRunner
{
	/// <summary>
	/// Event raised when progress is reported.
	/// </summary>
	event EventHandler<ProgressEventArgs> ProgressChanged;

	/// <summary>
	/// Event raised when standard output is written.
	/// </summary>
	event EventHandler<OutputEventArgs> OutputReceived;

	/// <summary>
	/// Event raised when standard error is written.
	/// </summary>
	event EventHandler<OutputEventArgs> ErrorReceived;
}
