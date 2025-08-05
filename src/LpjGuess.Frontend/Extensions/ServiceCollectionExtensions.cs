using System.Reflection;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Data;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Interfaces.Presenters;
using Microsoft.Extensions.DependencyInjection;

namespace LpjGuess.Frontend.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Register all presenters in the specified assembly.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assembly">The assembly to scan for presenters.</param>
    public static void AddPresenters(this IServiceCollection services, Assembly assembly)
    {
        // Get concrete presenter types.
        IEnumerable<Type> presenterTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => typeof(IPresenter).IsAssignableFrom(t));

        // Register non-generic presenter types.
        foreach (Type presenterType in presenterTypes)
        {
            if (presenterType.IsGenericType)
                services.AddGenericPresenter(presenterType);
            else
                services.AddPresenter(presenterType);
        }
    }

    /// <summary>
    /// Register a generic presenter type with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="presenterType">The generic presenter type to register.</param>
    private static void AddGenericPresenter(this IServiceCollection services, Type presenterType)
    {
        // Note: this currently only supports presenters with a single type
        // argument. We should rethink this.
        if (presenterType.GetGenericArguments().Length != 1)
            throw new InvalidOperationException($"Presenter type {presenterType.ToFriendlyName()} is generic but has more than one type argument");

        GenericPresenterAttribute? attribute = presenterType.GetCustomAttribute<GenericPresenterAttribute>();
        if (attribute == null)
            throw new InvalidOperationException($"Presenter type {presenterType.ToFriendlyName()} is generic but has no {nameof(GenericPresenterAttribute)} attribute");

        foreach (Type typeArgument in attribute.SupportedTypes)
        {
            Type genericType = presenterType.MakeGenericType(typeArgument);
            RegisterPresenterAttribute? attr = genericType.GetCustomAttribute<RegisterPresenterAttribute>();
            if (attr != null && attr.ModelType.IsGenericType)
            {
                Type modelType = attr.ModelType.MakeGenericType(typeArgument);
                services.AddModelPresenter(genericType, attr.InterfaceType, modelType);
            }
            else
            {
                services.AddPresenter(genericType);
            }
        }
    }

    /// <summary>
    /// Register a presenter type with the dependency injection container. The
    /// presenter type cannot be an open generic type.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="presenterType">The presenter type to register.</param>
    private static void AddPresenter(this IServiceCollection services, Type presenterType)
    {
        RegisterPresenterAttribute? attribute = presenterType.GetCustomAttribute<RegisterPresenterAttribute>();
        if (attribute == null)
        {
            RegisterStandalonePresenterAttribute? standaloneAttribute = presenterType.GetCustomAttribute<RegisterStandalonePresenterAttribute>();
            if (standaloneAttribute == null)
                throw new InvalidOperationException($"Presenter type {presenterType.ToFriendlyName()} has no {nameof(RegisterPresenterAttribute)} or {nameof(RegisterStandalonePresenterAttribute)} attribute");

            services.AddService(standaloneAttribute.InterfaceType, presenterType);
            return;
        }

        Type modelType = attribute.ModelType;
        Type interfaceType = attribute.InterfaceType;
        services.AddModelPresenter(presenterType, interfaceType, modelType);
    }

    private static void AddModelPresenter(this IServiceCollection services, Type presenterType, Type interfaceType, Type modelType)
    {
        // Only register a factory; don't register the presenter itself.
        Type factoryInterfaceType = typeof(IModelPresenterFactory<,>).MakeGenericType(interfaceType, modelType);
        Type factoryType = typeof(ModelPresenterFactory<,,>).MakeGenericType(interfaceType, presenterType, modelType);
        services.AddService(factoryInterfaceType, factoryType);
    }

    private static void AddService(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        Console.WriteLine($"services.AddTransient<{serviceType.ToFriendlyName()}, {implementationType.ToFriendlyName()}>();");
        services.AddTransient(serviceType, implementationType);
    }
}
