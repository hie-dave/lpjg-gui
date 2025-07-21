namespace LpjGuess.Frontend.DependencyInjection;

using LpjGuess.Frontend.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

/// <summary>
/// Utility class for scanning assemblies for types.
/// </summary>
public static class AssemblyScanner
{
    /// <summary>
    /// Add all implementations of the specified type in the given assembly to
    /// the service collection.
    /// </summary>
    /// <typeparam name="T">The type to find implementations of.</typeparam>
    /// <param name="services">The service collection to add the implementations to.</param>
    /// <param name="assembly">The assembly to scan for implementations.</param>
    public static void AddInAssembly<T>(this IServiceCollection services, Assembly assembly)
    {
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
                Console.WriteLine($"services.AddTransient<{interfaceType.ToFriendlyName()}, {implementations[0].ToFriendlyName()}>();");
                services.AddTransient(interfaceType, implementations[0]);
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
                    Console.WriteLine($"services.AddTransient<{concreteInterface.ToFriendlyName()}, {implementation.ToFriendlyName()}>();");
                    services.AddTransient(concreteInterface, implementation);
                }
                if (others.Count == 0)
                    Console.WriteLine($"Found un-implemented generic view interface: {interfaceType.ToFriendlyName()}");
            }
            else
            {
                Console.WriteLine($"Found un-implemented view interface: {interfaceType.ToFriendlyName()}");
            }
        }
    }
}
