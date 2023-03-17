namespace LpjGuess.Core.Interfaces;

/// <summary>
/// An interface to a class containing configuration settings which define how
/// to run a particular lpj-guess simulation (e.g. input module, .ins file path,
/// etc).
/// </summary>
public interface ISimulation
{
	/// <summary>
	/// Path to the instruction file.
	/// </summary>
	string InsFile { get; }

	/// <summary>
	/// Name of the input module to be used.
	/// </summary>
	/// <remarks>
	/// todo: use an enum here.
	/// </remarks>
	string InputModule { get; }
}
