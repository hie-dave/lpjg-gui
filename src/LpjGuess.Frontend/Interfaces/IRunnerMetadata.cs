namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// Metadata for a runner.
/// </summary>
public interface IRunnerMetadata
{
	/// <summary>
	/// Name of the runner.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Is this the default runner?
	/// </summary>
	bool IsDefault { get; }
}
