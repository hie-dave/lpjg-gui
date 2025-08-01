using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Graphing;

namespace LpjGuess.Core.Models;

/// <summary>
/// LPJ-Guess workspace.
/// </summary>
[Serializable]
public class Workspace
{
	/// <summary>
	/// The default output directory for generated simulations.
	/// </summary>
	private const string defaultOutputDirectory = ".simulations";

	/// <summary>
	/// The default file extension used for workspace files.
	/// </summary>
	public const string DefaultFileExtension = ".lpj";

	/// <summary>
	/// Instruction file path.
	/// </summary>
	public List<string> InstructionFiles { get; set; }

	/// <summary>
	/// Path to this workspace.
	/// </summary>
	public string FilePath { get; set; }

	/// <summary>
	/// List of graphs configured by the user.
	/// </summary>
	public List<Graph> Graphs { get; set; }

	/// <summary>
	/// List of experiments configured by the user.
	/// </summary>
	public List<Experiment> Experiments { get; set; }

	/// <summary>
	/// Default constructor provided for deserialization purposes only.
	/// </summary>
	public Workspace()
	{
		InstructionFiles = [];
		FilePath = string.Empty;
		Graphs = [];
		Experiments = [];
	}

	/// <summary>
	/// Create a new <see cref="Workspace"/> instance for the specified instruction
	/// file and serialise to disk before returning. The lpj file will be saved
	/// in the same directory as the .ins file, but with the default lpj file
	/// (not ins) extension.
	/// </summary>
	/// <param name="insFile">Path to the instruction file.</param>
	public static Workspace ForInsFile(string insFile)
	{
		Workspace result = new Workspace();
		result.Experiments.Add(Experiment.CreateBaseline());
		result.InstructionFiles.Add(insFile);
		result.FilePath = Path.ChangeExtension(insFile, DefaultFileExtension);
		return result;
	}

    /// <summary>
    /// Get the output directory for generated simulations.
    /// </summary>
    /// <returns>The output directory.</returns>
    public string GetOutputDirectory()
    {
		string directory = Path.GetDirectoryName(FilePath)
			?? Directory.GetCurrentDirectory();
        return Path.Combine(directory, defaultOutputDirectory);
    }
}
