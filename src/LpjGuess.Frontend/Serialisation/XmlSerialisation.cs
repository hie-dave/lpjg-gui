using System.Xml;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using LpjGuess.Core.Runners.Configuration;

namespace LpjGuess.Frontend.Serialisation;

/// <summary>
/// XML serialisation/deserialisation methods.
/// </summary>
public static class XmlSerialisation
{
	/// <summary>
	/// Serialise the given object to the specified file.
	/// </summary>
	/// <param name="obj">The object to be serialised.</param>
	/// <param name="file">File name and path to which config will be saved.</param>
	public static void SerialiseTo(this object obj, string file)
	{
		try
		{
			string? dir = Path.GetDirectoryName(file);
			if (dir == null)
				throw new Exception($"'{file}' must be an absolute path");

			// Create directory if it does not exist.
			Directory.CreateDirectory(dir);

			using (TextWriter writer = new StreamWriter(file, false))
				SerialiseTo(obj, writer);
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to save config to '{file}'", error);
		}
	}

	/// <summary>
	/// Serialise the given object to the specified stream.
	/// </summary>
	/// <param name="obj">The object to be serialised.</param>
	/// <param name="stream">The stream to which the object will be serialised.</param>
	/// <param name="leaveOpen">Iff true, the stream will be left open after serialisation.</param>
	public static void SerialiseTo(this object obj, Stream stream, bool leaveOpen = false)
	{
		using (TextWriter writer = new StreamWriter(stream, leaveOpen: leaveOpen))
			SerialiseTo(obj, writer);
	}

	/// <summary>
	/// Serialise the given object to the specified text writer.
	/// </summary>
	/// <param name="obj">The object to be serialised.</param>
	/// <param name="writer">The stream writer.</param>
	public static void SerialiseTo(this object obj, TextWriter writer)
	{
		using (XmlWriter xmlWriter = XmlWriter.Create(writer, GetSerialisationSettings()))
		{
			IExtendedXmlSerializer serialiser = CreateSerialiser();
			serialiser.Serialize(xmlWriter, obj);
		}
	}

	/// <summary>
	/// Load configuration from the specified file.
	/// </summary>
	/// <param name="file">Input file to be read.</param>
	public static T DeserialiseFrom<T>(string file) where T : new()
	{
		try
		{
			using (TextReader reader = new StreamReader(file))
				return DeserialiseFrom<T>(reader);
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to deserialise from '{file}'", error);
		}
	}

	/// <summary>
	/// Load configuration from the specified stream reader.
	/// </summary>
	/// <param name="stream">The input stream.</param>
	/// <param name="leaveOpen">Iff true, the input stream will be left open after deserialisation.</param>
	public static T DeserialiseFrom<T>(Stream stream, bool leaveOpen = false) where T : new()
	{
		using (StreamReader reader = new StreamReader(stream, leaveOpen: leaveOpen))
			return DeserialiseFrom<T>(reader);
	}

	/// <summary>
	/// Load configuration from the specified stream reader.
	/// </summary>
	/// <param name="reader">The input stream reader.</param>
	public static T DeserialiseFrom<T>(TextReader reader) where T : new()
	{
		XmlReaderSettings settings = GetDeserialisationSettings();
		using (XmlReader xmlReader = XmlReader.Create(reader, settings))
		{
			IExtendedXmlSerializer serialiser = CreateSerialiser();
			object? result = serialiser.Deserialize(xmlReader);
			if (result is T inst)
				return inst;

			throw new Exception($"Failed to deserialise {typeof(T).Name}");
		}
	}

	/// <summary>
	/// Get XML serialiser with standard configuration.
	/// </summary>
	private static IExtendedXmlSerializer CreateSerialiser()
	{
		return new ConfigurationContainer()
			// .UseAutoFormatting()
			.EnableImplicitTyping(typeof(Configuration))
			// .EnableImplicitTypingFromNamespace<IRunnerConfiguration>()
			.EnableImplicitTypingFromAll<LocalRunnerConfiguration>()
			.Create();
	}

	/// <summary>
	/// Get standard settings used for serialisation.
	/// </summary>
	private static XmlWriterSettings GetSerialisationSettings()
	{
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		return settings;
	}

	/// <summary>
	/// Get standard settings used for deserialisation.
	/// </summary>
	private static XmlReaderSettings GetDeserialisationSettings()
	{
		XmlReaderSettings settings = new XmlReaderSettings();
		return settings;
	}
}
