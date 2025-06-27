using Microsoft.Extensions.DependencyInjection;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Views;
using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Interfaces.Presenters;

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
        services.AddSingleton<IPresenterFactory, PresenterFactory>();
        services.AddSingleton<IViewFactory, ViewFactory>();
        services.AddSingleton(Configuration.Instance);

        // Register views.
        // TODO: make this configurable.
        IUIToolkitProvider provider = new Gtk4Provider();
        provider.RegisterViews(services);

        // Register presenters.
        services.AddInAssembly<IPresenter>(typeof(MainPresenter).Assembly);
    }

    /// <inheritdoc />
    public T GetService<T>() where T : notnull
    {
        return serviceProvider.GetRequiredService<T>();
    }
}
