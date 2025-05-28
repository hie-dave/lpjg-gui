using System.Text.Json;
using System.Text.Json.Serialization;
using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Runner.Models;

namespace LpjGuess.Core.Serialisation.Json;

/// <summary>
/// JSON serialisation/deserialisation methods.
/// </summary>
public static class JsonSerialisation
{
    /// <summary>
    /// Default JSON serialization options.
    /// </summary>
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(),
            // Add polymorphic converters for interfaces
            new PolymorphicConverter<IRunnerConfiguration>(),
            new PolymorphicConverter<ISeries>(),
            new PolymorphicConverter<IDataSource>(),
            new PolymorphicConverter<IStyleProvider<Colour>>(),
            new PolymorphicConverter<IStyleProvider<LineThickness>>(),
            new PolymorphicConverter<IStyleProvider<LineType>>(),
            // Add more interface converters as needed
        }
    };

    /// <summary>
    /// Serialise the given object to the specified file.
    /// </summary>
    /// <param name="obj">The object to be serialised.</param>
    /// <param name="file">File name and path to which config will be saved.</param>
    /// <param name="options">Optional custom serialization options.</param>
    public static void SerialiseTo(this object obj, string file, JsonSerializerOptions? options = null)
    {
        try
        {
            string? dir = Path.GetDirectoryName(file);
            if (dir == null)
                throw new ArgumentException($"Invalid file path: '{file}'");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(obj, options ?? DefaultOptions);
            File.WriteAllText(file, json);
        }
        catch (Exception error)
        {
            throw new Exception($"Unable to serialise to '{file}'", error);
        }
    }

    /// <summary>
    /// Load configuration from the specified file.
    /// </summary>
    /// <param name="file">Input file to be read.</param>
    /// <param name="options">Optional custom serialization options.</param>
    public static T DeserialiseFrom<T>(string file, JsonSerializerOptions? options = null) where T : new()
    {
        try
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"File not found: '{file}'", file);

            string json = File.ReadAllText(file);
            T? result = JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
            
            return result ?? new T();
        }
        catch (Exception error)
        {
            throw new Exception($"Unable to deserialise from '{file}'", error);
        }
    }

    /// <summary>
    /// Create a deep clone of an object using JSON serialization.
    /// </summary>
    /// <typeparam name="T">The type of object to clone.</typeparam>
    /// <param name="source">The source object to clone.</param>
    /// <param name="options">Optional custom serialization options.</param>
    /// <returns>A deep clone of the source object.</returns>
    public static T DeepClone<T>(T source, JsonSerializerOptions? options = null)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        string json = JsonSerializer.Serialize(source, options ?? DefaultOptions);
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions) ?? 
               throw new InvalidOperationException("Failed to deserialize cloned object");
    }
}
