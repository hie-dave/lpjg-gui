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
    public static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole(options =>
            {
                options.FormatterName = CustomConsoleFormatterOptions.FormatterName;
            });
            logging.AddConsoleFormatter<CustomConsoleFormatter, CustomConsoleFormatterOptions>(options =>
            {
                options.TimestampFormat = "HH:mm:ss ";
                options.IncludeScopes = true;
            });
        });

        return services;
    }
}
