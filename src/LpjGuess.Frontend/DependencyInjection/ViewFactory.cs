using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// A factory for creating views.
/// </summary>
public class ViewFactory : IViewFactory
{
    /// <summary>
    /// The service provider.
    /// </summary>
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Create a new <see cref="ViewFactory"/> instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ViewFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public T CreateView<T>() where T : IView
    {
        return serviceProvider.GetRequiredService<T>();
    }
}
