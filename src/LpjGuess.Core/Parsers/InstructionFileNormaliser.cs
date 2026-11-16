using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("LpjGuess.Tests")]

namespace LpjGuess.Core.Parsers;

/// <summary>
/// Encapsulates an instruction file.
/// </summary>
public class InstructionFileNormaliser
{
	/// <summary>
	/// Top-level parameters to be normalised. E.g. "file_met_forcing".
	/// </summary>
	private static readonly string[] parameters =
	[
		"file_met_forcing",
		"file_met_spinup",
		"file_gridlist",
		"file_soildata",
		"file_co2",
		"file_disturb",
		"file_thome"
	];

	/// <summary>
	/// Parameters within a "param" block to be normalised. E.g.
	/// param "file_soildata" (str "path.txt")
	/// </summary>
	private static readonly string[] @params =
	[
		"file_soildata",
		"file_ndep",
		"file_soildepth",
		"file_soilslope",
		"file_soilclay",
		"file_soilsand",
		"file_soilcoarse",
		"file_soilpH",
		"file_simfire",
		"file_disturbance",
		"file_disturbance_st",
		"file_elevation_st",
		"file_lu",
		"file_grossLUC",
		"file_lucrop",
		"file_lupasture",
		"file_lunatural",
		"file_luforest",
		"file_woodharv_frac",
		"file_woodharv_cmass",
		"file_Nfert_st",
		"file_cutinterval_st",
		"file_firstmanageyear_st"
	];

	/// <summary>
	/// List of import directives.
	/// </summary>
	private List<string> importDirectives;

	/// <summary>
	/// Raw content of the instruction file.
	/// </summary>
	private string rawContent;

	/// <summary>
	/// Path to the input file.
	/// </summary>
	public string FilePath { get; private init; }

	/// <summary>
	/// Create a new <see cref="InstructionFileNormaliser"/> instance.
	/// </summary>
	/// <param name="path">Path to the instruction file.</param>
	internal InstructionFileNormaliser(string path)
	{
		FilePath = path;
		rawContent = File.ReadAllText(path);
		importDirectives = new List<string>();
	}

	/// <summary>
	/// Normalise the instruction file at the specified path and return the
	/// contents of the normalised file.
	/// </summary>
	/// <remarks>
	/// Normalisation consists of:
	///
	/// - Flattening the import hierarchy, recursively
	/// - Converting all relative paths to absolute paths
	/// </remarks>
	/// <param name="path">Path to the instruction file.</param>
	/// <returns>Contents of the normalised file.</returns>
	public static string Normalise(string path)
	{
		InstructionFileNormaliser ins = new(path);
		ins.Flatten();
		ins.ConvertToAbsolutePaths();
		return ins.Read();
	}

	/// <summary>
	/// Get all import directives in the instruction file tree recursively.
	/// </summary>
	/// <param name="path">Path to the instruction file.</param>
	/// <returns>Absolute paths to imported instruction files.</returns>
	public static IEnumerable<string> ResolveImportDirectives(string path)
	{
		InstructionFileNormaliser ins = new(path);
		ins.Flatten();
		return ins.importDirectives;
	}

	/// <summary>
	/// Copy the text of all imported .ins files, recursively, into this file.
	/// </summary>
	/// <returns>Contents of the flattened file.</returns>
	private void Flatten()
	{
		string pattern = $@"^[ \t]*import[ \t]+""([^""]+)"".*\n";
		RegexOptions opts = RegexOptions.Multiline;
		Match match;
		while ((match = Regex.Match(rawContent, pattern, opts)) != Match.Empty)
		{
			string file = match.Groups[1].Value;
			string absolutePath = GetAbsolutePath(file);
			importDirectives.Add(absolutePath);
			rawContent = rawContent.Remove(match.Index, match.Length);
			InstructionFileNormaliser normaliser = new(absolutePath);
			normaliser.Flatten();
			importDirectives.AddRange(normaliser.importDirectives);
			rawContent = rawContent.Insert(match.Index, normaliser.Read());
		}
	}

	/// <summary>
	/// Get the absolute path of a file.
	/// </summary>
	/// <param name="file">Path to the file.</param>
	/// <returns>Path to the file, absolute.</returns>
    private string GetAbsolutePath(string file)
    {
		if (Path.IsPathRooted(file))
			return file;

        string relative = Path.GetDirectoryName(FilePath) ?? Directory.GetCurrentDirectory();
		if (!Path.IsPathRooted(relative))
			relative = Path.GetFullPath(relative, Directory.GetCurrentDirectory());
		return Path.GetFullPath(file, relative);
    }

    /// <summary>
    /// Change any relative paths to absolute paths.
    /// </summary>
    /// <returns>Contents of the file with absolute paths.</returns>
    private void ConvertToAbsolutePaths()
	{
		if (string.IsNullOrWhiteSpace(rawContent))
			return;

		// Use the content constructor: FromFile would normalise the file again.
		InstructionFileParser parser = new(rawContent, FilePath);

		// TODO: add support for CF input? Would need to add params like
		// file_temp, file_prec, file_insol, etc.
		foreach (string parameter in parameters)
		{
			string? value = parser.GetTopLevelParameterValue(parameter);
			if (string.IsNullOrEmpty(value))
				continue;
			string absolute = GetAbsolutePath(value);
			if (absolute != value)
				parser.SetTopLevelParameterValue(parameter, absolute);
		}

		foreach (string parameter in @params)
			NormaliseBlockPath(parser, "param", parameter, "str");

		foreach (string name in parser.GetBlockNames("file"))
			NormaliseBlockPath(parser, "file", name, "path");

		rawContent = parser.GenerateContent();
	}

	/// <summary>
	/// Normalise a path parameter within a block, if it is present and nonempty.
	/// </summary>
	private void NormaliseBlockPath(
		InstructionFileParser parser,
		string blockType,
		string blockName,
		string parameter)
	{
		string? value = parser.GetBlockParameterValue(blockType, blockName, parameter);
		if (string.IsNullOrEmpty(value))
			return;
		string absolute = GetAbsolutePath(value);
		if (absolute != value)
			parser.SetBlockParameterValue(blockType, blockName, parameter, $"\"{absolute}\"");
	}

	internal string Read()
	{
		return rawContent;
	}
}
