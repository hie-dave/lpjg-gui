using ExtendedXmlSerializer;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Serialisation;

namespace LpjGuess.Core.Models;

/// <summary>
/// LPJ-Guess ins file workspace.
/// </summary>
[Serializable]
public class LpjFile
{
	/// <summary>
	/// The default file extension used for these workspace files.
	/// </summary>
	private const string defaultFileExtension = ".lpj";

	/// <summary>
	/// Instruction file path.
	/// </summary>
	public string InstructionFile { get; set; }

	/// <summary>
	/// Path to this lpj file.
	/// </summary>
	public string FilePath { get; set; }

	/// <summary>
	/// List of graphs configured by the user.
	/// </summary>
	public List<Graph> Graphs { get; set; }

	/// <summary>
	/// Default constructor provided for deserialization purposes only.
	/// </summary>
	public LpjFile()
	{
		InstructionFile = string.Empty;
		FilePath = string.Empty;
		Graphs = new List<Graph>();
	}

	public void Save()
	{
		this.SerialiseTo(FilePath);
	}

	/// <summary>
	/// Create a new <see cref="LpjFile"/> instance for the specified instruction
	/// file and serialise to disk before returning. The lpj file will be saved
	/// in the same directory as the .ins file, but with the default lpj file
	/// (not ins) extension.
	/// </summary>
	/// <param name="insFile">Path to the instruction file.</param>
	public static LpjFile ForInsFile(string insFile)
	{
		LpjFile result = new LpjFile();
		result.InstructionFile = insFile;
		result.FilePath = Path.ChangeExtension(insFile, defaultFileExtension);
		result.Save();
		return result;
	}

	/// <summary>
	/// Deserialise the specified file into an instance of <see cref="LpjFile"/>.
	/// </summary>
	/// <param name="filePath">Path to the serialised file.</param>
	public static LpjFile LoadFrom(string filePath)
	{
		return XmlSerialisation.DeserialiseFrom<LpjFile>(filePath);
	}
}
