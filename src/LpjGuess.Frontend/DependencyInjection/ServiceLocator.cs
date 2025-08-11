using Microsoft.Extensions.DependencyInjection;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Runner.Services;
using LpjGuess.Frontend.Services;
using LpjGuess.Frontend.Data;
using LpjGuess.Frontend.Utility;
using LpjGuess.Core.Models;
using Microsoft.Extensions.Logging;
using LpjGuess.Frontend.Interfaces.Factories;
using LpjGuess.Core.Logging;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// Service locator for dependency injection.
/// </summary>
public class ServiceLocator : IServiceLocator
{
    /// <summary>
    /// The service provider.
    /// </summary>
    private static readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initialize the service locator.
    /// </summary>
    static ServiceLocator()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Configure the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Register services.
        services.AddSingleton<ICommandRegistry, CommandRegistry>();
        services.AddScoped<IPresenterFactory, PresenterFactory>();
        services.AddSingleton(Configuration.Instance);

        // Add scoped services.
        services.AddScoped<IInstructionFilesProvider, InstructionFilesProvider>();
        services.AddScoped<IExperimentProvider, ExperimentProvider>();
        services.AddScoped<IPathResolver, WorkspacePathResolver>();

        // Register transient services.
        services.AddTransient<WorkspacePresenterFactory>();
        services.AddTransient<IViewFactory, ViewFactory>();
        services.AddTransient<IDataProviderFactory, DataProviderFactory>();
        services.AddTransient<OxyPlotConverter>();
        services.AddTransient<IDataProvider<ModelOutput>, ModelOutputReader>();
        services.AddTransient<ModelOutputReader>();

        // Configure logging to use our custom console formatter.
        services.ConfigureLogging();

        // Register views.
        // TODO: make this configurable.
        IUIToolkitProvider provider = new Gtk4Provider();
        provider.RegisterViews(services);

        // Register presenters.
        services.AddPresenters(typeof(MainPresenter).Assembly);
    }

    /// <inheritdoc />
    public T GetService<T>() where T : notnull
    {
        return serviceProvider.GetRequiredService<T>();
    }
}
