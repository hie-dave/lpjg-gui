using Microsoft.Extensions.Logging.Console;

namespace LpjGuess.Core.Logging;

/// <summary>
/// Options for the <see cref="CustomConsoleFormatter"/>.
/// </summary>
public class CustomConsoleFormatterOptions : ConsoleFormatterOptions
{
    /// <summary>
    /// The name of the formatter.
    /// </summary>
    public const string FormatterName = "Custom";
}
