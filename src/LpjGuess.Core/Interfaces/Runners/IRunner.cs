namespace LpjGuess.Core.Interfaces.Runners;

/// <summary>
/// Interface for a class which can run the model.
/// </summary>
public interface IRunner : IDisposable
{
	/// <summary>
	/// Start running the model.
	/// </summary>
	void Run();

	/// <summary>
	/// Cancel a run of the model.
	/// </summary>
	void Cancel();

	/// <summary>
	/// Check if the model is running.
	/// </summary>
	bool IsRunning { get; }
}
