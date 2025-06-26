using Adw;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// GTK4 UI toolkit provider.
/// </summary>
public class Gtk4Provider : IUIToolkitProvider
{
    /// <inheritdoc />
    public void RegisterViews(IServiceCollection services)
    {
        // services.AddTransient<IExperimentView, ExperimentView>();
        // services.AddTransient<IFactorialView, FactorialView>();
        services.AddInAssembly<IView>(typeof(MainView).Assembly);
        services.AddSingleton<IApplication, Gtk4Application>();
    }
}
