using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LpjGuess.Runner.Models;

namespace LpjGuess.Frontend.Serialisation.Json;

/// <summary>
/// JSON serialisation/deserialisation methods.
/// </summary>
public static class JsonSerialisation
{
    /// <summary>
    /// Default JSON serialization settings.
    /// </summary>
    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto,
        Converters = 
        {
            new StringEnumConverter()
        }
    };

    /// <summary>
    /// Serialise the given object to the specified file.
    /// </summary>
    /// <param name="obj">The object to be serialised.</param>
    /// <param name="file">File name and path to which config will be saved.</param>
    /// <param name="settings">Optional custom serialization settings.</param>
    public static void SerialiseTo(this object obj, string file, JsonSerializerSettings? settings = null)
    {
        try
        {
            string? dir = Path.GetDirectoryName(file);
            if (dir == null)
                throw new ArgumentException($"Invalid file path: '{file}'");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string json = JsonConvert.SerializeObject(obj, settings ?? DefaultSettings);
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
    /// <param name="settings">Optional custom serialization settings.</param>
    public static T DeserialiseFrom<T>(string file, JsonSerializerSettings? settings = null) where T : new()
    {
        try
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"File not found: '{file}'", file);

            string json = File.ReadAllText(file);
            T? result = JsonConvert.DeserializeObject<T>(json, settings ?? DefaultSettings);
            
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
    /// <param name="settings">Optional custom serialization settings.</param>
    /// <returns>A deep clone of the source object.</returns>
    public static T DeepClone<T>(T source, JsonSerializerSettings? settings = null)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        string json = JsonConvert.SerializeObject(source, settings ?? DefaultSettings);
        return JsonConvert.DeserializeObject<T>(json, settings ?? DefaultSettings) ??
               throw new InvalidOperationException("Failed to deserialize cloned object");
    }
}
