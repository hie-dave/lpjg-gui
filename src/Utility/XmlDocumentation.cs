using System.Reflection;
using System.Xml;

namespace LpjGuess.Frontend.Utility;

/// <summary>
/// Helper methods for reading from XML documentation.
/// </summary>
public static class XmlDocumentation
{
	/// <summary>
	/// Cache of assemblies for which xml documentation has already been loaded.
	/// </summary>
	private static Dictionary<Assembly, XmlDocument> documents = new Dictionary<Assembly, XmlDocument>();

	/// <summary>
	/// Get the XML documentation summary of a member.
	/// </summary>
	/// <param name="member">The member.</param>
	public static string GetSummary(this MemberInfo member)
	{
		// Apparently member.DeclaringType will be null for global members.
		if (member.DeclaringType == null)
			throw new InvalidOperationException($"Unable to get documentation for global member {member.Name}");

		string path = GetMemberPath(member);
		char typeChar = GetTypeChar(member);
		XmlDocument doc = LoadDocumentation(member.DeclaringType.Assembly);
		return GetSummary(path, typeChar, doc);
	}

	/// <summary>
	/// Get the path to the specified member in the form
	/// "Assembly.Namespace.Type.Member".
	/// </summary>
	/// <param name="member">A member.</param>
	private static string GetMemberPath(MemberInfo member)
	{
		string fullName = member.ReflectedType + "." + member.Name;
		if (member is MethodInfo method)
		{
			string args = string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName));
			fullName = $"{fullName}({args})";
		}
		fullName = fullName.Replace("+", ".");
		return fullName;
	}

	/// <summary>
	/// Get the type character for the specified member. See also:
	/// <see cref="GetSummary(string, char, XmlDocument)"/>.
	/// </summary>
	/// <param name="member">A member.</param>
	private static char GetTypeChar(MemberInfo member)
	{
		if (member is PropertyInfo)
			return 'P';
		else if (member is FieldInfo)
			return 'F';
		else if (member is EventInfo)
			return 'E';
		else if (member is MethodInfo method)
			return 'M';
		else
			throw new ArgumentException($"Unknown member type: {member.GetType().Name}");
	}

	/// <summary>
	/// Get the XML documentation summary of the specified type.
	/// </summary>
	/// <param name="t">A type.</param>
	public static string GetSummary(this Type t)
	{
		if (t.FullName == null)
			throw new InvalidOperationException($"Unable to get fullname of type {t.Name}");
		return GetSummary(t.FullName.Replace("+", ""), 'T', LoadDocumentation(t.Assembly));
	}

	/// <summary>
	/// Get the summary of the item specified by a type letter. This should be
	/// one of:
	/// 
	/// - 'P' (property)
	/// - 'F' (field)
	/// - 'E' (event)
	/// - 'M' (method)
	/// - 'T' (type)
	/// 
	/// The path should be of the form: "Assembly.Namespace.Type".
	/// </summary>
	/// <param name="path">Path of the member within the document.</param>
	/// <param name="type">The type character.</param>
	/// <param name="doc">The XML document loaded from the assembly documentation file.</param>
	private static string GetSummary(string path, char type, XmlDocument doc)
	{
		string xpath = $"//member[@name = \"{type}:{path}\"]/summary";
		XmlNode? node = doc.SelectSingleNode(xpath);
		if (node == null)
			throw new InvalidOperationException($"XML documentation for item does not exist");
		return node.InnerText.Trim();
	}

	/// <summary>
	/// Get the path to the directory containing the specified assembly.
	/// </summary>
	/// <param name="assembly">An assembly.</param>
	private static string GetDirectory(this Assembly assembly)
	{
		string? dir = Path.GetDirectoryName(assembly.Location);
		if (dir == null)
			throw new Exception($"Unable to get directory of assembly {assembly.FullName}");
		return dir;
	}

	/// <summary>
	/// Load documentation for the specified assembly.
	/// </summary>
	/// <param name="assembly">An assembly.</param>
	private static XmlDocument LoadDocumentation(Assembly assembly)
	{
		// Load from cache if possible.
		if (documents.ContainsKey(assembly))
			return documents[assembly];

		// Get path to the directory containing the assembly.
		string dir = assembly.GetDirectory();

		// Get assembly name.
		string? name = assembly.GetName().Name;
		if (name == null)
			throw new InvalidOperationException($"Assembly {assembly.FullName} has no name");

		// Get xml documentation file name.
		string file = Path.Combine(dir, assembly.GetName().Name + ".xml");
		if (!File.Exists(file))
			throw new InvalidOperationException($"XML Documentation file {file} does not exist");

		// Load file, store in cache, return document.
		XmlDocument doc = new XmlDocument();
		doc.Load(file);
		documents.Add(assembly, doc);
		return doc;
	}
}
