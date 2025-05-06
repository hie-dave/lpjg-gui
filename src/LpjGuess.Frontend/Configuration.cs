using System.Xml.Serialization;
using ExtendedXmlSerializer;
using LpjGuess.Runner.Models;
using LpjGuess.Core.Serialisation;

namespace LpjGuess.Frontend;

/// <summary>
/// User-customisable configuration settings, and code to save/load to disk.
/// </summary>
/// <remarks>
/// todo:
/// - refactor serialisation code out of here.
/// - ?Add options for serialisation method?
/// </remarks>
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
		try
		{
			Instance = Load();
		}
		catch (Exception error)
		{
			Instance = new Configuration();
			Console.Error.WriteLine(error);
		}
	}

	/// <summary>
	/// Default constructor, intended for internal use only, and used to
	/// generate a default configuration when no config file exists.
	/// </summary>
	public Configuration()
	{
	}

	/// <summary>
	/// The current configuration instance. This is loaded from disk the first
	/// time this class is accessed.
	/// </summary>
	public static Configuration Instance { get; private set; }

	/// <summary>
	/// True to prefer dark mode. False to use system theme.
	/// </summary>
	// [UI("Prefer dark mode")]
	public bool PreferDarkMode { get; set; }

	/// <summary>
	/// If true, the "logs" tab will be automatically selected whenever a
	/// simulation is run.
	/// </summary>
	public bool GoToLogsTabOnRun { get; set; }

	/// <summary>
	/// Maximum number of workspaces to remember in "recent workspaces".
	/// </summary>
	public int MaxHistoryLength { get; set; }

	/// <summary>
	/// List of recent workspaces.
	/// </summary>
	public HashSet<string> RecentWorkspaces { get; set; } = new();

	/// <summary>
	/// Runner configurations provided by the user.
	/// </summary>
	// [UI("Runners")]
	public List<IRunnerConfiguration> Runners { get; set; } = new();

	/// <summary>
	/// Get the default runner configuration.
	/// </summary>
	public int DefaultRunnerIndex { get; set; }

	/// <summary>
	/// Save current configuration to disk in the default location.
	/// </summary>
	public void Save()
	{
		this.SerialiseTo(GetConfigPath(), c => c.EnableImplicitTyping(typeof(Configuration)));
	}

	/// <summary>
	/// Load configuration from disk at the default path.
	/// </summary>
	private static Configuration Load()
	{
		string path = GetConfigPath();
		// Config file may not exist the first time the application is run.
		if (!File.Exists(path))
			return new Configuration();
		return XmlSerialisation.DeserialiseFrom<Configuration>(path, c => c.EnableImplicitTyping(typeof(Configuration)));
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
