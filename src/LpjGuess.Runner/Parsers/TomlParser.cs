using System.Globalization;
using LpjGuess.Runner.Extensions;
using LpjGuess.Runner.Models;
using Tomlyn;
using Tomlyn.Helpers;
using Tomlyn.Model;

namespace LpjGuess.Runner.Parsers;

/// <summary>
/// This class can parse a .toml input file.
/// </summary>
internal class TomlParser : IParser
{
	/// <inheritdoc />
	public SimulationGenerator Parse(string file)
	{
		try
		{
			TomlTable model = Toml.ToModel(File.ReadAllText(file));
			return Parse(model);
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to parse input file '{file}'", error);
		}
	}

	/// <summary>
	/// Parse an instructions object from the raw input object. This requires
	/// some manual validation of inputs not covered by the library.
	/// </summary>
	/// <param name="model">The raw user input object.</param>
	private SimulationGenerator Parse(TomlTable model)
	{
		RunSettings settings = ParseRunSettings(model);
		IReadOnlyCollection<Factorial> combinations = ParseParameters(model, settings.FullFactorial);
		IReadOnlyCollection<string> pfts = ParsePfts(model);
		IReadOnlyCollection<string> insFiles = ParseInsFiles(model);

		return new SimulationGenerator(insFiles, pfts, combinations, settings);
	}

	private IReadOnlyCollection<string> ParseInsFiles(TomlTable model)
	{
		const string key = "insfiles";
		return ParseStringArray(model, key, false);
	}

	private IReadOnlyCollection<string> ParsePfts(TomlTable model)
	{
		const string key = "pfts";
		return ParseStringArray(model, key, true);
	}

	private RunSettings ParseRunSettings(TomlTable model)
	{
		bool fullFactorial = ParseBool(model, "full_factorial");
		bool dryRun = ParseBool(model, "dry_run", optional: true);
		bool runLocal = ParseBool(model, "run_local");

		ushort cpuCount = ParseUshort(model, "cpu_count");
		uint memory = ParseUint(model, "memory");
		TimeSpan walltime = ParseTimeSpan(model, "walltime");

		string queue = ParseString(model, "queue");
		string project = ParseString(model, "project");
		string jobName = ParseString(model, "job_name");

		string outputDirectory = ParseString(model, "output_directory");
		string guessPath = ParseString(model, "guess_path");
		string inputModule = ParseString(model, "input_module");

		// Email address is optional iff emailNotifications is false.
		bool emailNotifications = ParseBool(model, "email_notifications");
		string emailAddress = ParseString(model, "email_address", !emailNotifications);

		return new RunSettings(
			dryRun,
			runLocal,
			outputDirectory,
			guessPath,
			inputModule,
			cpuCount,
			walltime,
			memory,
			queue,
			project,
			emailNotifications,
			emailAddress,
			jobName,
			fullFactorial
		);
	}

	/// <summary>
	/// Parse all factorial settings from the model.
	/// </summary>
	/// <param name="model">The model.</param>
	private IReadOnlyCollection<Factorial> ParseParameters(TomlTable model, bool fullFactorial)
	{
		const string keyParameters = "parameters";
		if (!model.ContainsKey(keyParameters))
			return new List<Factorial>();

		if ( !(model[keyParameters] is TomlTable section) )
			throw new InvalidOperationException($"{keyParameters} should be a section");

		Dictionary<string, List<string>> parameters = new Dictionary<string, List<string>>();
		foreach ((string key, object value) in section)
		{
			if (section[key] is TomlTable table)
				foreach ((string fullKey, List<string> values) in ParseTableOfArrays(table, key))
					parameters[fullKey] = values;
			else
				parameters[key] = ParseStringArray(section, key).ToList();
		}

		return GetParameters(parameters, fullFactorial);
	}

	/// <summary>
	/// Get all combinations of factors from the user inputs.
	/// </summary>
	/// <param name="parameters">The parameters as they appear in the user input object.</param>
	private IReadOnlyCollection<Factorial> GetParameters(IDictionary<string, List<string>> parameters, bool fullFactorial)
	{
		if (parameters.Count == 0)
			return new List<Factorial>();

		// Convert dictionary to 2D list of factors.
		List<List<Factor>> factors = new List<List<Factor>>();
		foreach ((string key, IReadOnlyList<string> values) in parameters)
			factors.Add([.. values.Select(v => new Factor(key, v))]);

		// Return all combinations thereof.
		List<List<Factor>> combinations;
		if (fullFactorial)
			combinations = factors.AllCombinations();
		else
			combinations = factors.SelectMany(f => f.Select(f => new List<Factor> { f })).ToList();
		return combinations.Select(c => new Factorial(c)).ToList();
	}

	private IReadOnlyDictionary<string, List<string>> ParseTableOfArrays(TomlTable table, string keyName)
	{
		Dictionary<string, List<string>> data = new();

		foreach (string key in table.Keys)
		{
			if (table[key] is TomlArray array)
			{
				List<string> values = ParseStringArray(table, key, false).ToList();
				string fullKey = $"{keyName}.{key}";
				data[fullKey] = values;
			}
		}
		return data;
	}

	/// <summary>
	/// Parse a string array from the model. If the key doesn't exist, an
	/// exception will be thrown, unless optional is set to true, in which case
	/// an empty list will be returned. This function will also throw if the
	/// array is empty (unless optional is true).
	/// </summary>
	/// <param name="model">The model.</param>
	/// <param name="keyName">Name of the string array.</param>
	/// <param name="optional">Is the array optional?</param>
	private IReadOnlyCollection<string> ParseStringArray(TomlTable model, string keyName, bool optional = false)
	{
		if (!model.ContainsKey(keyName))
		{
			if (optional)
				return Array.Empty<string>();
			throw new InvalidOperationException(VarNotSet(keyName));
		}

		// TBI - add support for scalar as single-element list.
		if ( !(model[keyName] is TomlArray array) )
			throw new InvalidOperationException($"{keyName} must be an array");

		if (array.Count == 0 && !optional)
			throw new InvalidOperationException($"{keyName} is empty");

		List<string> values = array
			.Select(i => i?.ToString())
			.Where(i => !string.IsNullOrEmpty(i))
			.Cast<string>()
			.Select(i => i.Trim())
			.ToList();

		if (values.Count == 0 && !optional)
			throw new InvalidOperationException($"No values provided for {keyName}");

		return values;
	}

	/// <summary>
	/// Parse a string value from the model. If the key doesn't exist, an
	/// exception will be thrown, unless optional is set to true, in which case
	/// an empty string will be returned.
	/// </summary>
	/// <param name="model">Toml model.</param>
	/// <param name="key">Key name.</param>
	/// <param name="optional">Is this value optional?</param>
	private string ParseString(TomlTable model, string key, bool optional = false)
	{
		if (!model.ContainsKey(key))
		{
			if (optional)
				// Return default value.
				return string.Empty;
			else
				throw new InvalidOperationException($"{key} not set");
		}

		if ( !(model[key] is string result) )
			throw new InvalidOperationException($"{key} must be a string (ie quoted) value");
		return result;
	}

	/// <summary>
	/// Parse a <see cref="TimeSpan"/> from the model.
	/// </summary>
	/// <param name="model">The model.</param>
	/// <param name="key">Name of the timespan.</param>
	private TimeSpan ParseTimeSpan(TomlTable model, string key)
	{
		string str = ParseString(model, key);
		if (!TimeSpan.TryParseExact(str, "c", null, out TimeSpan result))
			throw new InvalidOperationException($"Invalid timespan value: '{str}' in input file");
		return result;
	}

	/// <summary>
	/// Parse a boolean value from the input file with the given key name. If
	/// the key does not exist, an exception will be thrown, unless optional is
	/// set to true, in which case false will be returned.
	/// </summary>
	/// <param name="model">Parsed input object.</param>
	/// <param name="keyName">Key name.</param>
	/// <param name="optional">Is this parameter optional?</param>
	private bool ParseBool(TomlTable model, string keyName, bool optional = false)
	{
		if (!model.ContainsKey(keyName))
		{
			if (optional)
				// Return default value.
				return false;
			else
				throw new InvalidOperationException(VarNotSet(keyName));
		}

		object value = model[keyName];
		if (!(value is bool result))
			throw new InvalidOperationException($"{keyName} must be boolean (true/false)");

		return result;
	}

	/// <summary>
	/// Parse uint value from the input file with the given key name. If the key
	/// does not exist, an exception will be thrown, unless optional is set to
	/// true, in which case false will be returned.
	/// </summary>
	/// <param name="model">Parsed input object.</param>
	/// <param name="keyName">Key name.</param>
	/// <param name="optional">Is this parameter optional?</param>
	private uint ParseUint(TomlTable model, string keyName, bool optional = false)
	{
		if (!model.ContainsKey(keyName))
		{
			if (optional)
				// Return default value.
				return 0;
			else
				throw new InvalidOperationException(VarNotSet(keyName));
		}

		string? value = model[keyName].ToString();
		if (value == null || !uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint result))
			throw new InvalidOperationException($"{keyName} must be uint (0..{uint.MaxValue})");

		return result;
	}

	/// <summary>
	/// Parse uint value from the input file with the given key name. If the key
	/// does not exist, an exception will be thrown, unless optional is set to
	/// true, in which case false will be returned.
	/// </summary>
	/// <param name="model">Parsed input object.</param>
	/// <param name="keyName">Key name.</param>
	/// <param name="optional">Is this parameter optional?</param>
	private ushort ParseUshort(TomlTable model, string keyName, bool optional = false)
	{
		if (!model.ContainsKey(keyName))
		{
			if (optional)
				// Return default value.
				return 0;
			else
				throw new InvalidOperationException(VarNotSet(keyName));
		}

		string? value = model[keyName].ToString();
		if (value == null || !ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort result))
			throw new InvalidOperationException($"{keyName} must be ushort (0..{ushort.MaxValue})");

		return result;
	}

	/// <summary>
	/// Return an appropriate error message for when a variable is not set in
	/// the input file.
	/// </summary>
	/// <param name="variable">Name of the variable.</param>
	private string VarNotSet(string variable)
	{
		string name = TomlNamingHelper.PascalToSnakeCase(variable);
		return $"Variable '{name}' is not set in input file";
	}
}
