using LpjGuess.Core.Interfaces;

namespace LpjGuess.Core.Models;

/// <summary>
/// An interface to a class containing configuration settings which define how
/// to run a particular lpj-guess simulation (e.g. input module, .ins file path,
/// etc).
/// </summary>
public class SimulationConfiguration : ISimulation
{
	/// <summary>
	/// Path to the instruction file.
	/// </summary>
	public string InsFile { get; private init; }

	/// <summary>
	/// Name of the input module to be used.
	/// </summary>
	/// <remarks>
	/// todo: use an enum here.
	/// </remarks>
	public string InputModule { get; private init; }

	/// <summary>
	/// Create a new <see cref="SimulationConfiguration"/> instance.
	/// </summary>
	/// <param name="insFile">The instruction file for this simulation.</param>
	/// <param name="inputModule">The input module to be used.</param>
	public SimulationConfiguration(string insFile, string inputModule)
	{
		InsFile = insFile;
		InputModule = inputModule;
	}
}
