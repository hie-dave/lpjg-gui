using LpjGuess.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Tests.Core.Logging;

public class LoggingConfigurationTests
{
    [Fact]
    public void LoggingOptions_Defaults_AreExpected()
    {
        var options = new LoggingOptions();

        Assert.Equal(LogLevel.Information, options.Verbosity);
        Assert.Equal("HH:mm:ss ", options.TimestampFormat);
        Assert.True(options.IncludeScopes);
        Assert.Empty(options.Filters);
    }

    [Fact]
    public void ConfigureLogging_RegistersLoggerFactory_AndAppliesFilters()
    {
        var services = new ServiceCollection();
        var options = new LoggingOptions
        {
            Verbosity = LogLevel.Warning,
            IncludeScopes = false,
            TimestampFormat = "HH:mm:ss ",
            Filters = [new LogFilter("Special.Category", LogLevel.Debug)]
        };

        IServiceCollection returned = services.ConfigureLogging(options);
        using ServiceProvider provider = services.BuildServiceProvider();
        ILoggerFactory factory = provider.GetRequiredService<ILoggerFactory>();

        Assert.Same(services, returned);

        ILogger defaultLogger = factory.CreateLogger("Other.Category");
        ILogger filteredLogger = factory.CreateLogger("Special.Category");

        Assert.False(defaultLogger.IsEnabled(LogLevel.Information));
        Assert.True(filteredLogger.IsEnabled(LogLevel.Debug));
    }
}
