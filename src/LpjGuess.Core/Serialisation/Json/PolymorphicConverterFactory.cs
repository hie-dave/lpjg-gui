using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LpjGuess.Core.Serialisation.Json;

/// <summary>
/// A JSON converter factory that creates polymorphic converters for interface types.
/// </summary>
public class PolymorphicConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// The assemblies to search for implementations.
    /// </summary>
    private readonly HashSet<Assembly> assemblies;

    private readonly HashSet<Assembly> notSupportedAssemblies;

    /// <summary>
    /// The cache of converters.
    /// </summary>
    private readonly ConcurrentDictionary<Type, JsonConverter> converterCache = new();


    /// <summary>
    /// Creates a new instance of the PolymorphicConverterFactory.
    /// </summary>
    /// <param name="assemblies">Optional assemblies to search for implementations. If not provided, the assembly containing the factory will be used.</param>
    public PolymorphicConverterFactory(params Assembly[] assemblies)
    {
        this.assemblies = [.. assemblies];
        this.assemblies.Add(GetType().Assembly); // LpjGuess.Core
        this.assemblies.Add(typeof(string).Assembly); // System.Private.CoreLib

        notSupportedAssemblies = [];
        notSupportedAssemblies.Add(typeof(string).Assembly);
    }

    /// <summary>
    /// Determines whether this converter can convert the specified type.
    /// </summary>
    public override bool CanConvert(Type typeToConvert)
    {
        // Only convert generic types, interfaces, and abstract types.
        // All other types can be handled by the default converter.
        if (notSupportedAssemblies.Contains(typeToConvert.Assembly))
            return false;
        return typeToConvert.IsGenericType || typeToConvert.IsInterface || typeToConvert.IsAbstract;
    }

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // Use cached converter if available.
        if (converterCache.TryGetValue(typeToConvert, out JsonConverter? cachedConverter))
            return cachedConverter;

        // Create a generic PolymorphicConverter for the type.
        Type converterType = typeof(PolymorphicConverter<>).MakeGenericType(typeToConvert);
        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            converterType,
            BindingFlags.Instance | BindingFlags.Public,
            null,
            [assemblies.ToArray()],
            null)!;

        // Cache the converter.
        converterCache[typeToConvert] = converter;
        return converter;
    }
}
