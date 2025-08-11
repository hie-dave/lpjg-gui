using Microsoft.Extensions.Logging;

namespace LpjGuess.Core.Logging;

/// <summary>
/// Options for logging.
/// </summary>
public class LoggingOptions
{
    /// <summary>
    /// The verbosity of the logger.
    /// </summary>
    public LogLevel Verbosity { get; init; } = LogLevel.Information;

    /// <summary>
    /// The format of the timestamp.
    /// </summary>
    public string TimestampFormat { get; init; } = "HH:mm:ss ";

    /// <summary>
    /// Whether to include scopes in the log output.
    /// </summary>
    public bool IncludeScopes { get; init; } = true;

    /// <summary>
    /// The log filters.
    /// </summary>
    public IEnumerable<LogFilter> Filters { get; init; } = [];
}
