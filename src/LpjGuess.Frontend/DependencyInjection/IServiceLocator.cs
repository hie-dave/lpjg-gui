namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// Interface to a service locator for dependency injection.
/// </summary>
public interface IServiceLocator
{
    /// <summary>
    /// Get a service from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance.</returns>
    T GetService<T>() where T : notnull;
}
