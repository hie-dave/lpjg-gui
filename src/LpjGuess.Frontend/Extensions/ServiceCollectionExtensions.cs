using System.Reflection;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Interfaces.Presenters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly Lock loggerLock = new();
    private static ILogger logger = null!;

    private static void InitLogger(IServiceCollection services)
    {
        if (logger == null)
        {
            lock (loggerLock)
            {
                if (logger == null)
                {
                    IServiceProvider serviceProvider = services.BuildServiceProvider();
                    logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ServiceCollectionExtensions");
                }
            }
        }
    }

    /// <summary>
    /// Register all presenters in the specified assembly.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assembly">The assembly to scan for presenters.</param>
    public static void AddPresenters(this IServiceCollection services, Assembly assembly)
    {
        InitLogger(services);

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
    /// Add all implementations of the specified type in the given assembly to
    /// the service collection.
    /// </summary>
    /// <typeparam name="T">The type to find implementations of.</typeparam>
    /// <param name="services">The service collection to add the implementations to.</param>
    /// <param name="assembly">The assembly to scan for implementations.</param>
    public static void AddInAssembly<T>(this IServiceCollection services, Assembly assembly)
    {
        InitLogger(services);

        // services.AddTransient<IExperimentView, ExperimentView>();
        // services.AddTransient<IFactorialView, FactorialView>();

        // TODO: what about generic types?

        // Find all interfaces that inherit from T.
        List<Type> interfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && typeof(T).IsAssignableFrom(t) && t != typeof(T))
            .ToList();

        foreach (Type interfaceType in interfaces)
        {
            var implementations = assembly.GetTypes()
                .Where(t => !t.IsAbstract &&
                       interfaceType.IsAssignableFrom(t))
                .ToList();

            if (implementations.Count == 1)
            {
                // Register the single implementation
                services.AddService(interfaceType, implementations[0]);
            }
            else if (interfaceType.IsGenericType)
            {
                List<Type> others = assembly.GetTypes()
                    .Where(t => !t.IsAbstract &&
                       t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType))
                    .ToList();
                foreach (Type implementation in others)
                {
                    Type concreteInterface = implementation.GetInterfaces()
                        .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
                    services.AddService(concreteInterface, implementation);
                }
                if (others.Count == 0)
                    logger.LogWarning($"Found un-implemented generic view interface: {interfaceType.ToFriendlyName()}");
            }
            else if (implementations.Count > 1)
            {
                // If exactly one implementation has the DefaultImplementation attribute, register it.
                Type? defaultImplementation = implementations
                    .FirstOrDefault(i => i.GetCustomAttribute<DefaultImplementationAttribute>() != null);
                if (defaultImplementation != null)
                {
                    services.AddService(interfaceType, defaultImplementation);
                }
                else
                {
                    logger.LogWarning("Found {0} implementations of {1}: {2}. These will not be registered with this interface.",
                        implementations.Count,
                        interfaceType.ToFriendlyName(),
                        string.Join(", ", implementations.Select(t => t.ToFriendlyName())));
                }
            }
            else
            {
                logger.LogWarning("Found un-implemented view interface: {0}", interfaceType.ToFriendlyName());
            }
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
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ServiceCollectionExtensions");
        logger.LogInformation("services.AddTransient<{0}, {1}>();", serviceType.ToFriendlyName(), implementationType.ToFriendlyName());
        services.AddTransient(serviceType, implementationType);
    }
}
