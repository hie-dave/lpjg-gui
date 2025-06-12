namespace LpjGuess.Runner.Models;

/// <summary>
/// Standard output/error event data.
/// </summary>
public class OutputEventArgs : EventArgs
{
	/// <summary>
	/// Name of the job which wrote the message.
	/// </summary>
	public string JobName { get; private init; }

	/// <summary>
	/// The data written by the model.
	/// </summary>
	public string Data { get; private init; }

	/// <summary>
	/// Create a new <see cref="OutputEventArgs" /> instance.
	/// </summary>
	/// <param name="jobName">Name of the job which wrote the message.</param>
	/// <param name="data">The data written by the model.</param>
	public OutputEventArgs(string jobName, string data)
	{
		JobName = jobName;
		Data = data;
	}
}
