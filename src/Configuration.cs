using LpjGuess.Frontend.Attributes;
using System.Xml.Serialization;

namespace LpjGuess.Frontend;

/// <summary>
/// User-customisable configuration settings, and code to save/load to disk.
/// </summary>
public class Configuration
{
	/// <summary>
	/// Name of the directory which contains the config file.
	/// </summary>
	private const string appDir = "lpjg-gui";

	/// <summary>
	/// Name of the config file.
	/// </summary>
	private const string fileName = "config.xml";

	/// <summary>
	/// Load the configuration from disk the first time this class is accessed.
	/// </summary>
	static Configuration()
	{
		Instance = Load();
	}

	/// <summary>
	/// Default constructor, intended for internal use only, and used to
	/// generate a default configuration when no config file exists.
	/// </summary>
	private Configuration()
	{
	}

	/// <summary>
	/// The current configuration instance. This is loaded from disk the first
	/// time this class is accessed.
	/// </summary>
	public static Configuration Instance { get; private set; }

	/// <summary>
	/// Path to a custom guess executable. If not set, the current directory
	/// and then PATH will be searched.
	/// </summary>
	[FileName("Custom guess executable")]
	public string? CustomExecutable { get; set; }

	/// <summary>
	/// True to prefer dark mode. False to use system theme.
	/// </summary>
	[UI("Prefer dark mode")]
	public bool PreferDarkMode { get; set; }

	/// <summary>
	/// Save current configuration to disk in the default location.
	/// </summary>
	public void Save()
	{
		SaveTo(GetConfigPath());
	}

	/// <summary>
	/// Save current configuration to disk at the specified location.
	/// </summary>
	/// <param name="file">File name and path to which config will be saved.</param>
	private void SaveTo(string file)
	{
		try
		{
			string? dir = Path.GetDirectoryName(file);
			if (dir == null)
				throw new Exception($"'{file}' must be an absolute path");

			// Create directory if it does not exist.
			Directory.CreateDirectory(dir);

			using (TextWriter writer = new StreamWriter(file, false))
			{
				XmlSerializer serialiser = new XmlSerializer(typeof(Configuration));
				serialiser.Serialize(writer, this);
			}
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to save config to '{file}'", error);
		}
	}

	/// <summary>
	/// Load configuration from disk at the default path.
	/// </summary>
	private static Configuration Load()
	{
		return LoadFrom(GetConfigPath());
	}

	/// <summary>
	/// Load configuration from the specified file.
	/// </summary>
	/// <param name="file">Configuration file to be read.</param>
	private static Configuration LoadFrom(string file)
	{
		try
		{
			string? path = Path.GetDirectoryName(file);
			if (path == null)
				throw new Exception($"'{fileName}' must be an absolute path");

			// Create the directory if it doesn't already exist.
			Directory.CreateDirectory(path);

			if (File.Exists(file))
			{
				XmlSerializer serialiser = new XmlSerializer(typeof(Configuration));
				using (TextReader reader = new StreamReader(file))
				{
					object? result = serialiser.Deserialize(reader);
					if (result is Configuration config)
						return config;

					// If we get to here, deserialisation failed. Therefore, we
					// create a default instance.
				}
			}

			// No configuration file exists on disk. This will happen the first time
			// this application is run on a particular machine. In this case, we
			// create a new configuration instance with default values.
			return new Configuration();
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to load configuration from '{file}'", error);
		}
	}

	/// <summary>
	/// Get the default path to the configuration file.
	/// </summary>
	private static string GetConfigPath()
	{
		var location = Environment.SpecialFolder.LocalApplicationData;
		return Path.Combine(
			Environment.GetFolderPath(location),
			appDir,
			fileName);
	}
}
