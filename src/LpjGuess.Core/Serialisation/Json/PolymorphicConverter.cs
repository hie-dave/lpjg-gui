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
        this.assemblies.Add(GetType().Assembly); // LpjGuess.Core
        this.assemblies.Add(typeof(string).Assembly);
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

        Type resolvedType = ParseTypeName(typeDiscriminator);

        TypeCache[typeDiscriminator] = resolvedType;
        return resolvedType;
    }

    /// <summary>
    /// Parse the name to a known type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    private Type ParseTypeName(string typeName)
    {
        // Check if this is a generic type
        if (typeName.Contains('<') && typeName.EndsWith('>'))
        {
            return ParseGenericTypeName(typeName);
        }

        // This is a non-generic type, find it in the assemblies
        foreach (var assembly in assemblies)
        {
            Type? type = assembly.GetTypes().FirstOrDefault(t =>
                SanitiseTypeName(t) == typeName);

            if (type != null)
                return type;
        }

        throw new JsonException($"Could not find type: {typeName}");
    }

    private Type ParseGenericTypeName(string typeName)
    {
        // Parse the base name and generic arguments
        int openBracketIndex = typeName.IndexOf('<');
        string baseName = typeName.Substring(0, openBracketIndex);
        string argsString = typeName.Substring(openBracketIndex + 1, typeName.Length - openBracketIndex - 2);

        // Parse the arguments, handling nested generics
        List<string> argNames = new List<string>();
        int nestLevel = 0;
        int startPos = 0;

        for (int i = 0; i < argsString.Length; i++)
        {
            char c = argsString[i];

            if (c == '<')
                nestLevel++;
            else if (c == '>')
                nestLevel--;
            else if (c == ',' && nestLevel == 0)
            {
                // Found a top-level argument separator
                argNames.Add(argsString.Substring(startPos, i - startPos).Trim());
                startPos = i + 1;
            }
        }

        // Add the last argument
        if (startPos < argsString.Length)
            argNames.Add(argsString.Substring(startPos).Trim());

        // Recursively resolve each generic argument.
        Type[] genericArgs = argNames.Select(ParseTypeName).ToArray();

        // The prefix with which the runtime type's Name must begin.
        string pfx = $"{baseName}`";

        // Find the generic type definition.
        foreach (Assembly assembly in assemblies)
        {
            // Restrict the search to generic types which have the prefix.
            IEnumerable<Type> candidates = assembly.GetTypes()
                .Where(t => t.IsGenericTypeDefinition && t.Name.StartsWith(pfx));
            foreach (Type type in candidates)
            {
                try
                {
                    // Check if this type can be constructed with the
                    // required generic type arguments.
                    if (genericArgs.Length != type.GetGenericArguments().Length)
                        continue;

                    Type constructed = type.MakeGenericType(genericArgs);
                    if (GetDiscriminatorFromType(constructed) == typeName)
                        return constructed;
                }
                catch
                {
                    // If this type can't be constructed with the required type
                    // parameters, it's not the type we're looking for.
                }
            }
        }

        throw new JsonException($"Could not find generic type definition for: {baseName}");
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
        string discriminator = SanitiseTypeName(type);

        // Add to cache and return
        DiscriminatorCache[type] = discriminator;
        return discriminator;
    }

    /// <summary>
    /// Sanitise a type name for JSON serialisation. This is required for
    /// generic types, as the default type.Name is not parse-able during
    /// deserialization.
    /// </summary>
    /// <param name="type">The type to sanitise.</param>
    /// <returns>The sanitised type name.</returns>
    private string SanitiseTypeName(Type type)
    {
        string name = type.Name;

        if (type.IsGenericType)
        {
            // Get the base name without the `1 suffix
            string baseName = name.Split('`')[0];

            // Get the generic type arguments
            Type[] genericArgs = type.GetGenericArguments();

            // Format as BaseName<Arg1,Arg2,...>
            string args = string.Join(",", genericArgs.Select(SanitiseTypeName));
            name = $"{baseName}<{args}>";
        }

        return name;
    }
}
