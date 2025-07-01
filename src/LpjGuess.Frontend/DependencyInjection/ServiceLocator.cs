using Microsoft.Extensions.DependencyInjection;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Views;
using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Interfaces.Presenters;
using System.Reflection;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Attributes;

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
        services.AddSingleton(Configuration.Instance);

        // Add scoped services.
        services.AddScoped<IInstructionFilesProvider, InstructionFilesProvider>();

        services.AddTransient<WorkspacePresenterFactory>();

        // Register views.
        // TODO: make this configurable.
        IUIToolkitProvider provider = new Gtk4Provider();
        provider.RegisterViews(services);

        // Register presenters.
        AddPresenters(services, typeof(MainPresenter).Assembly);
    }

    /// <inheritdoc />
    public T GetService<T>() where T : notnull
    {
        return serviceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Register all presenters in the specified assembly.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assembly">The assembly to scan for presenters.</param>
    private static void AddPresenters(IServiceCollection services, Assembly assembly)
    {
        // Get concrete presenter types.
        IEnumerable<Type> presenterTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => typeof(IPresenter).IsAssignableFrom(t));

        // Register non-generic presenter types.
        foreach (Type presenterType in presenterTypes.Where(t => !t.IsGenericType))
        {
            // Register with all implemented interfaces.
            foreach (Type interfaceType in presenterType.GetInterfaces()
                .Where(i => typeof(IPresenter).IsAssignableFrom(i) && i != typeof(IPresenter)))
            {
                services.AddTransient(interfaceType, presenterType);
                Console.WriteLine($"services.AddTransient<{interfaceType.ToFriendlyName()}, {presenterType.ToFriendlyName()}>();");
            }

            // Also register with IPresenter.
            services.AddTransient(typeof(IPresenter), presenterType);
            Console.WriteLine($"services.AddTransient<IPresenter, {presenterType.ToFriendlyName()}>();");
        }

        foreach (Type presenterType in presenterTypes.Where(t => t.IsGenericType))
        {
            RegisterGenericType(services, presenterType);
        }
    }

    /// <summary>
    /// Register a generic type with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register the type with.</param>
    /// <param name="presenterType">The generic type to register.</param>
    private static void RegisterGenericType(IServiceCollection services, Type presenterType)
    {
        IEnumerable<Type> supportedTypes = GetSupportedTypeParameters(presenterType);
        IReadOnlyList<Type> interfaceTypes = presenterType.GetInterfaces()
            .Where(i => typeof(IPresenter).IsAssignableFrom(i) && i != typeof(IPresenter))
            .ToList();

        foreach (Type type in supportedTypes)
        {
            Type genericType = presenterType.MakeGenericType(type);
            foreach (Type interfaceType in interfaceTypes)
            {
                Type concreteInterfaceType = Concretise(interfaceType, type);
                Console.WriteLine($"services.AddTransient<{concreteInterfaceType.ToFriendlyName()}, {genericType.ToFriendlyName()}>();");
                services.AddTransient(concreteInterfaceType, genericType);
            }
            Console.WriteLine($"services.AddTransient<IPresenter, {genericType.ToFriendlyName()}>();");
            services.AddTransient(typeof(IPresenter), genericType);
        }
    }

    /// <summary>
    /// Concretise the specified type. If it's not generic, the original type
    /// will be returned. If it is an open generic type, it will be closed by
    /// using the specified type as the type argument.
    /// </summary>
    /// <param name="interfaceType"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static Type Concretise(Type interfaceType, Type type)
    {
        if (!interfaceType.IsGenericType)
            // Non-generic type.
            return interfaceType;

        if (!interfaceType.ContainsGenericParameters)
            // Closed generic type.
            return interfaceType;

        if (interfaceType.IsGenericTypeDefinition)
            return interfaceType.MakeGenericType(type);

        // Handle partially closed generics (like IPresenter<DiscreteValues<T>>)
        Type genericTypeDef = interfaceType.GetGenericTypeDefinition();
        Type[] genericArgs = interfaceType.GetGenericArguments();

        // Replace any generic parameters with our concrete type
        for (int i = 0; i < genericArgs.Length; i++)
        {
            if (genericArgs[i].IsGenericParameter)
            {
                genericArgs[i] = type;
            }
            else if (genericArgs[i].IsGenericType && genericArgs[i].ContainsGenericParameters)
            {
                // Handle nested generic types (like DiscreteValues<T>)
                genericArgs[i] = Concretise(genericArgs[i], type);
            }
        }

        // Create the new closed generic type
        return genericTypeDef.MakeGenericType(genericArgs);
    }

    /// <summary>
    /// Get all supported type parameters for the specified generic type.
    /// </summary>
    /// <param name="presenterType">The generic type to get supported type parameters for.</param>
    /// <returns>The supported type parameters.</returns>
    private static IEnumerable<Type> GetSupportedTypeParameters(Type presenterType)
    {
        Type[] typeParams = presenterType.GetGenericArguments();
        if (typeParams.Length != 1)
        {
            LogWarning($"Type {presenterType.ToFriendlyName()} is generic on >1 type parameters. This will not be registered for dependency injection.");
            return Array.Empty<Type>();
        }

        var attribute = presenterType.GetCustomAttribute<RegisterGenericTypeAttribute>();
        if (attribute != null && attribute.SupportedTypes.Length > 0)
            return attribute.SupportedTypes;

        // If the type parameter is an interface, find all concrete types that
        // implement it.
        Type typeParam = typeParams[0];
        if (typeParam.IsInterface)
        {
            return presenterType.Assembly.GetTypes()
                .Where(t => !t.IsInterface &&
                            !t.IsAbstract &&
                            typeParam.IsAssignableFrom(t))
                .ToList();
        }

        return Array.Empty<Type>();
    }

    /// <summary>
    /// Log a warning message.
    /// </summary>
    /// <remarks>
    /// TODO: proper logging.
    /// </remarks>
    /// <param name="message">The message to log.</param>
    private static void LogWarning(string message)
    {
        Console.WriteLine($"Warning: {message}");
    }
}
