using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Parsers;

/// <summary>
/// Interface for a class which can parse an input file.
/// </summary>
internal interface IParser
{
	/// <summary>
	/// Parse an input file.
	/// </summary>
	/// <param name="file">Path to the input file.</param>
	RunnerConfiguration Parse(string file);
}
