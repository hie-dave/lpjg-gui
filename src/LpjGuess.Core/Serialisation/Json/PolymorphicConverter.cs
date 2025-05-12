using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using System.Collections.Concurrent;

namespace LpjGuess.Core.Serialisation.Json;

/// <summary>
/// A generic JSON converter that supports polymorphic serialization of interface or abstract types.
/// </summary>
/// <typeparam name="TInterface">The interface or abstract type to convert.</typeparam>
public class PolymorphicConverter<TInterface> : JsonConverter<TInterface> where TInterface : class
{
    private const string TypeDiscriminatorPropertyName = "$type";
    
    // Cache of type mappings for better performance
    private static readonly ConcurrentDictionary<string, Type> TypeCache = new();
    private static readonly ConcurrentDictionary<Type, string> DiscriminatorCache = new();
    
    // Assemblies to search for implementations
    private readonly HashSet<Assembly> assemblies;

    /// <summary>
    /// Creates a new instance of the PolymorphicConverter.
    /// </summary>
    /// <param name="assemblies">Optional assemblies to search for implementations in addition to the current assembly. If not provided, the assembly containing TInterface will be used.</param>
    public PolymorphicConverter(params Assembly[] assemblies)
    {
        this.assemblies = [..assemblies];
        this.assemblies.Add(GetType().Assembly);
        if (assemblies.Length == 0)
            this.assemblies.Add(typeof(TInterface).Assembly);
    }
    
    /// <summary>
    /// Read and convert the JSON to the interface type.
    /// </summary>
    public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        using (JsonDocument document = JsonDocument.ParseValue(ref reader))
        {
            if (!document.RootElement.TryGetProperty(TypeDiscriminatorPropertyName, out JsonElement typeProperty))
            {
                throw new JsonException($"Missing type discriminator property '{TypeDiscriminatorPropertyName}'");
            }

            string typeDiscriminator = typeProperty.GetString() ?? 
                throw new JsonException("Type discriminator cannot be null");

            // Get the concrete type based on the discriminator
            Type concreteType = GetTypeFromDiscriminator(typeDiscriminator);
            
            // Deserialize to the concrete type
            return (TInterface?)JsonSerializer.Deserialize(
                document.RootElement.GetRawText(), 
                concreteType, 
                options);
        }
    }

    /// <summary>
    /// Write an interface object to JSON.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }
        
        // Create a temporary options to avoid infinite recursion
        var serializerOptions = new JsonSerializerOptions(options)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        // Remove this converter to avoid infinite recursion
        foreach (var converter in serializerOptions.Converters.ToList())
        {
            if (converter is PolymorphicConverter<TInterface>)
            {
                serializerOptions.Converters.Remove(converter);
            }
        }
        
        // Serialize the object to a JsonDocument
        string valueJson = JsonSerializer.Serialize(value, value.GetType(), serializerOptions);
        using (JsonDocument document = JsonDocument.Parse(valueJson))
        {
            writer.WriteStartObject();
            
            // Write the type discriminator
            writer.WriteString(TypeDiscriminatorPropertyName, GetDiscriminatorFromType(value.GetType()));
            
            // Write all the properties from the original object
            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                property.WriteTo(writer);
            }
            
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// Get the concrete type from a type discriminator string.
    /// </summary>
    /// <param name="typeDiscriminator">The type discriminator string.</param>
    /// <returns>The concrete Type.</returns>
    private Type GetTypeFromDiscriminator(string typeDiscriminator)
    {
        // Check cache first
        if (TypeCache.TryGetValue(typeDiscriminator, out Type? cachedType))
        {
            return cachedType;
        }
        
        // Search for the type in the provided assemblies
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAbstract && !type.IsInterface && 
                    typeof(TInterface).IsAssignableFrom(type) && 
                    type.Name == typeDiscriminator)
                {
                    // Add to cache and return
                    TypeCache[typeDiscriminator] = type;
                    return type;
                }
            }
        }
        
        throw new JsonException($"Unknown type discriminator: {typeDiscriminator}");
    }

    /// <summary>
    /// Get the type discriminator string from a Type.
    /// </summary>
    /// <param name="type">The concrete type.</param>
    /// <returns>The type discriminator string.</returns>
    private string GetDiscriminatorFromType(Type type)
    {
        // Check cache first
        if (DiscriminatorCache.TryGetValue(type, out string? cachedDiscriminator))
        {
            return cachedDiscriminator;
        }
        
        // Use the type name as the discriminator
        string discriminator = type.Name;
        
        // Add to cache and return
        DiscriminatorCache[type] = discriminator;
        return discriminator;
    }
}
