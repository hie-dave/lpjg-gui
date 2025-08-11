using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Core.Logging;

/// <summary>
/// Provides centralized logging configuration.
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configure logging with a custom console formatter for the given service collection.
    /// </summary>
    /// <param name="services">The service collection to configure logging for.</param>
    /// <param name="options">The logging options.</param>
    public static IServiceCollection ConfigureLogging(this IServiceCollection services, LoggingOptions options)
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(options.Verbosity);
            logging.AddConsole(options =>
            {
                options.FormatterName = CustomConsoleFormatterOptions.FormatterName;
            });
            logging.AddConsoleFormatter<CustomConsoleFormatter, CustomConsoleFormatterOptions>(o =>
            {
                o.TimestampFormat = options.TimestampFormat;
                o.IncludeScopes = options.IncludeScopes;
            });
            foreach (LogFilter filter in options.Filters)
                logging.AddFilter(filter.Category, filter.Level);
        });

        return services;
    }
}
