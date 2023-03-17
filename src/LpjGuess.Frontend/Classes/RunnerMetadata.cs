using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Classes;

/// <summary>
/// Metadata required to display a runner row in the runner configuration page.
/// </summary>
public record RunnerMetadata : IRunnerMetadata
{
	/// <summary>
	/// Name of the runner.
	/// </summary>
	public string Name { get; private init; }

	/// <summary>
	/// True iff this is the default runner.
	/// </summary>
	public bool IsDefault { get; private init; }

	/// <summary>
	/// Create a new <see cref="RunnerMetadata"/> instance.
	/// </summary>
	/// <param name="name">Name of the runner.</param>
	/// <param name="isDefault">True iff this is the default runner.</param>
	public RunnerMetadata(string name, bool isDefault)
	{
		Name = name;
		IsDefault = isDefault;
	}
}
