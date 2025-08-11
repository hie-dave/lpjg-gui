using CommandLine;
using LpjGuess.Core.Logging;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Config;

/// <summary>
/// CLI Options.
/// </summary>
public class Options
{
    /// <summary>
    /// The logging level.
    /// </summary>
    private readonly LogLevel verbosity;

    /// <summary>
    /// The logging filters, used to specify verbosity on a per-class basis. May
    /// use a regex. Can be specified multiple times. Example value:
    /// "LpjGuess.Frontend.Views.*=Debug".
    /// </summary>
    private readonly IEnumerable<string> logFilters;

    /// <summary>
    /// Unparsed command line arguments.
    /// </summary>
    private readonly IEnumerable<string> unparsed;

    /// <summary>
    /// The logging level.
    /// </summary>
    [Option('l', "log-level", Default = LogLevel.Information, HelpText = "The logging verbosity (Trace, Debug, Information, Warning, Error, Critical).")]
    public LogLevel Verbosity => verbosity;

    /// <summary>
    /// The logging filters, used to specify verbosity on a per-class basis. May
    /// use a regex. Can be specified multiple times. Example value:
    /// "LpjGuess.Frontend.Views.*=Debug".
    /// </summary>
    [Option("log-filter", HelpText = "The logging filters, used to specify verbosity on a per-class basis. May use a regex. Can be specified multiple times. Example value: \"LpjGuess.Frontend.Views.*=Debug\"")]
    public IEnumerable<string> LogFilters => logFilters;

    /// <summary>
    /// Unparsed command line arguments.
    /// </summary>
    [Value(0)]
    public IEnumerable<string> Unparsed => unparsed;

    /// <summary>
    /// Create a new options object.
    /// </summary>
    /// <param name="verbosity">The logging level.</param>
    /// <param name="logFilters">The logging filters.</param>
    /// <param name="unparsed">Unparsed command line arguments.</param>
    public Options(
        LogLevel verbosity,
        IEnumerable<string> logFilters,
        IEnumerable<string> unparsed)
    {
        this.verbosity = verbosity;
        this.logFilters = logFilters;
        this.unparsed = unparsed;
    }

    /// <summary>
    /// The default options.
    /// </summary>
    public static Options Instance { get; private set; } = Default();

    /// <summary>
    /// Parse the command line arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The parsed options.</returns>
    public static Options Parse(string[] args)
    {
        Parser parser = new Parser(ConfigureParser);
        ParserResult<Options> result = parser.ParseArguments<Options>(args);
        if (result is not Parsed<Options> parsed)
            throw new Exception("Failed to parse command line arguments.");
        Instance = parsed.Value;
        return parsed.Value;
    }

    /// <summary>
    /// Configure the parser.
    /// </summary>
    /// <param name="settings">Parser settings.</param>
    private static void ConfigureParser(ParserSettings settings)
    {
        settings.AutoVersion = true;
        settings.EnableDashDash = true;
    }

    /// <summary>
    /// Initialize the default options.
    /// </summary>
    private static Options Default()
    {
        return new Options(LogLevel.Information, [], []);
    }

    /// <summary>
    /// Convert this options object to a <see cref="LoggingOptions"/>.
    /// </summary>
    /// <returns>The logging options.</returns>
    public LoggingOptions ToLoggingOptions()
    {
        return new LoggingOptions()
        {
            Verbosity = Verbosity,
            Filters = LogFilters.Select(LogFilter.Parse)
        };
    }
}
