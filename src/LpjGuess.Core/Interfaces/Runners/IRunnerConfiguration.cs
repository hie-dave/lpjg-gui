namespace LpjGuess.Core.Interfaces.Runners;

/// <summary>
/// An interface to a class which describes how to run lpj-guess. Data in this
/// class is not particular to running a specific simulation, but rather it
/// provides a way of running lpj-guess.
/// </summary>
public interface IRunnerConfiguration
{
	/// <summary>
	/// Path to the lpj-guess executable.
	/// </summary>
	/// <remarks>
	/// Does this belong in the interface?
	/// </remarks>
	string GuessPath { get; set; }

	/// <summary>
	/// Name of this runner (used for display purposes only).
	/// </summary>
	string Name { get; set; }
}
