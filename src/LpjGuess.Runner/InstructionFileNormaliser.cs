using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using LpjGuess.Runner.Models;

[assembly: InternalsVisibleTo("LpjGuess.Tests")]

namespace LpjGuess.Runner;

/// <summary>
/// Encapsulates an instruction file.
/// </summary>
/// <remarks>
/// This is really quick and dirty. This really should be improved because
/// performance is not going to be great here.
/// </remarks>
public class InstructionFileNormaliser
{
	/// <summary>
	/// Path to the input file.
	/// </summary>
	public string FilePath { get; private init; }

	/// <summary>
	/// List of import directives.
	/// </summary>
	private List<string> importDirectives;

	private string rawContent;

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
		ins.ConvertToAbsolutePaths();
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
		return Path.GetFullPath(file, relative);
    }

    /// <summary>
    /// Change any relative paths to absolute paths.
    /// </summary>
    /// <param name="contents">Contents of the instruction file.</param>
    /// <returns>Contents of the file with absolute paths.</returns>
    private void ConvertToAbsolutePaths()
	{
		string[] parameters = new[]
		{
			"file_met_forcing",
			"file_met_spinup",
			"file_gridlist",
			"file_soildata",
			"file_co2"
		};
		string[] paramsToFix = new[]
		{
			"file_soildata",
			"file_ndep"
		};
		// TODO: add support for CF input? Would need to add params like
		// file_temp, file_prec, file_insol, etc.
		foreach (string parameter in parameters)
		{
			string? value = TryGetParameterValue(parameter, rawContent);
			if (value == null)
				continue;
			string absolute = GetAbsolutePath(value);
			rawContent = SetParameterValue(parameter, absolute, rawContent);
		}
		foreach (string parameter in paramsToFix)
		{
			string? value = TryGetParamValue(parameter, rawContent);
			if (value == null)
				continue;
			string absolute = GetAbsolutePath(value);
			rawContent = SetParamValue(parameter, absolute, rawContent);
		}
	}

	internal string Read()
	{
		return rawContent;
	}

	/// <summary>
	/// Return the value of a param in the instruction file. Throw if it does
	/// not exist.
	/// </summary>
	/// <param name="name">Param name.</param>
	/// <param name="contents">Contents of the instruction file.</param>
	/// <returns>Value of the param.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the param does not exist.</exception>
	internal string GetParamValue(string name, string contents)
	{
		string? result = TryGetParamValue(name, contents);
		if (result == null)
			throw new InvalidOperationException(ParameterDoesNotExist(name));
		return result;
	}

	/// <summary>
	/// Return the value of a param value in the instruction file. If it does
	/// not exist, return null.
	/// </summary>
	/// <param name="name">Name of the param.</param>
	/// <param name="contents">Contents of the instruction file.</param>
	/// <returns>Value of the param, or null if it does not exist.</returns>
	internal string? TryGetParamValue(string name, string contents)
	{
		RegexOptions opts = RegexOptions.Multiline;
		string pattern = GetParamRegex(name);
		Match match = Regex.Match(contents, pattern, opts);
		if (match.Success)
			return match.Groups[2].Value;
		return null;
	}

	/// <summary>
	/// Modify the value of a 'param' option in the .ins file.
	/// </summary>
	/// <param name="name">Name of the parameter.</param>
	/// <param name="value">New value of the parameter.</param>
	internal string SetParamValue(string name, string value, string contents)
	{
		string previousValue = GetParamValue(name, contents);
		if (string.Equals(value, previousValue))
			// Parameter already has this value.
			return contents;

		RegexOptions opts = RegexOptions.Multiline;
		string pattern = GetParamRegex(name);
		string repl = $@"${{1}}{value}${{3}}";
		string newBuf = Regex.Replace(contents, pattern, repl, opts);
		if (string.Equals(newBuf, contents))
			throw new InvalidOperationException($"Unable to modify '{name}' param (replacement failed)");
		return newBuf;
	}

	/// <summary>
	/// Return a regex which may be used to match 'param' values in the
	/// instruction file. Ther return regex contains 3 capture groups:
	/// 1. Everything before the value.
	/// 2. The value.
	/// 3. Everything after the value.
	/// </summary>
	/// <param name="name">Name of the parameter.</param>
	/// <returns>Regex pattern for matching the parameter.</returns>
	private static string GetParamRegex(string name)
	{
		return $@"^([ \t]*param[ \t]+""{name}""[ \t]+\(str[ \t]+"")([^""]+)(""\).*)";
	}

	/// <summary>
	/// Get a parameter value in the instruction file. Throw if not found.
	/// </summary>
	/// <param name="name">Name of the parameter.</param>
	/// <param name="contents">Contents of the instruction file.</param>
	/// <returns>Value of the parameter.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the parameter does not exist.</exception>
	private string GetParameterValue(string name, string contents)
	{
		string? result = TryGetParameterValue(name, contents);
		if (result == null)
			throw new InvalidOperationException(ParameterDoesNotExist(name));
		return result;
	}

	/// <summary>
	/// Get a parameter value if it exists in the instruction file. If it does
	/// not exist, return null.
	/// </summary>
	/// <param name="name">Name of the parameter.</param>
	/// <param name="contents">Contents of the instruction file.</param>
	/// <returns>Value of the parameter, or null if it does not exist.</returns>
	internal string? TryGetParameterValue(string name, string contents)
	{
		RegexOptions opts = RegexOptions.Multiline;
		string pattern = GetParameterRegex(name);
		Match match = Regex.Match(contents, pattern, opts);
		if (match.Success)
			return match.Groups[2].Value;
		return null;
	}

	/// <summary>
	/// Change a parameter in the instruction file.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="value">Parameter value.</param>
	/// <param name="contents">Contents of the instruction file.</param>
	/// <returns>Contents of the instruction file with the parameter changed.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the parameter does not exist.</exception>
	private string SetParameterValue(string name, string value, string contents)
	{
		string previousValue = GetParameterValue(name, contents);
		if (string.Equals(value, previousValue))
			// Nothing to do - parameter already has this value.
			return contents;
		RegexOptions opts = RegexOptions.Multiline;
		string pattern = GetParameterRegex(name);
		string repl = $@"${{1}}{value}${{3}}";
		// todo: check if a match/replacement occurred.
		string replaced = Regex.Replace(contents, pattern, repl, opts);
		if (string.Equals(replaced, contents))
			throw new InvalidOperationException(ParameterDoesNotExist(name));
		return replaced;
	}

	/// <summary>
	/// Return a regex pattern which can be used to search for a string
	/// parameter. The pattern requires the MultiLine option toe be enabled and
	/// contains 3 capture groups:
	/// 1. Everything before the parameter value.
	/// 2. The parameter value.
	/// 3. Everything after the parameter value.
	/// </summary>
	/// <param name="name">Name of the parameter.</param>
	/// <returns>Regex pattern for matching the parameter.</returns>
	private string GetParameterRegex(string name)
	{
		return $@"^([ \t]*{name}[ \t]+""?)((?:[ \t]*[^!"" \n])+)(""?[ \t]*[^\n]*)?$";
	}

	/// <summary>
	/// Get a 'parameter does not exist' error message for the specified
	/// parameter.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <returns>Error message.</returns>
	private string? ParameterDoesNotExist(string name)
	{
		return $"Parameter '{name}' does not exist";
	}
}
