using Microsoft.Extensions.DependencyInjection;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// Interface for a provider of UI toolkit services.
/// </summary>
public interface IUIToolkitProvider
{
    /// <summary>
    /// Register views with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register views with.</param>
    void RegisterViews(IServiceCollection services);
}
