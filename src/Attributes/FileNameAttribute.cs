namespace LpjGuess.Frontend.Attributes;

/// <summary>
/// An enum for common file types which may be used to construct
/// <see cref="FileFilter"/> objects.
/// </summary>
public enum FileType
{
	/// <summary>
	/// All files.
	/// </summary>
	All,

	/// <summary>
	/// Executable files.
	/// </summary>
	Executables,
}

/// <summary>
/// A file filter.
/// </summary>
public struct FileFilter
{
	/// <summary>
	/// "All Files" pattern.
	/// </summary>
	private const string patternAll = "*.*";

	/// <summary>
	/// "All Files" name.
	/// </summary>
	private const string nameAll = "All Files";

	/// <summary>
	/// Executable files pattern (windows only).
	/// </summary>
	private const string patternExe = "*.exe";

	/// <summary>
	/// Executable files name.
	/// </summary>
	private const string nameExe = "Executable Files";

	/// <summary>
	/// Shell glob pattern (e.g. "*.sh").
	/// </summary>
	public string Pattern { get; private init; }

	/// <summary>
	/// Name of the filter (e.g. "Shell scripts").
	/// </summary>
	public string Name { get; private init; }

	/// <summary>
	/// Create a new <see cref="FileFilter"/> pattern.
	/// </summary>
	/// <param name="pattern">Filter pattern.</param>
	/// <param name="name">Name of the filter.</param>
	public FileFilter(string pattern, string name)
	{
		Pattern = pattern;
		Name = name;
	}

	/// <summary>
	/// Create a new <see cref="FileFilter"/> pattern from a known/common file
	/// type.
	/// </summary>
	/// <param name="type">The file type.</param>
	public static FileFilter FromKnownType(FileType type)
	{
		switch (type)
		{
			case FileType.Executables:
				return new FileFilter(patternExe, nameExe);
			case FileType.All:
				return new FileFilter(patternAll, nameAll);
			default:
				throw new InvalidOperationException($"Unknown file type {type}");
		}
	}
}

/// <summary>
/// This attribute, when applied to a string property, will cause the string
/// property to be rendered by a property presenter as a file name, and will
/// allow the user to change it via a file selector dialog.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FileNameAttribute : UIAttribute
{
	/// <summary>
	/// Filters used 
	/// </summary>
	/// <value></value>
	public IReadOnlyList<FileFilter> Filters { get; private init; }

	/// <summary>
	/// Create a new <see cref="FileNameAttribute"/> instance for the given
	/// file filters.
	/// </summary>
	/// <param name="name">Name of the property.</param>
	/// <param name="filters">
	/// Filters applied when selecting a suitable file in the GUI. Every second
	/// element should be a description of the previous element. E.g.
	/// "*.png", "PNG Images", "*.*", "All Files"
	/// </param>
	public FileNameAttribute(string name, params FileType[] filters) : base(name)
	{
		List<FileFilter> fileFilters = new List<FileFilter>();
		foreach (FileType type in filters)
		{
			if (type == FileType.Executables)
			{
#if !WINDOWS
				// Don't add a "*.exe" filter if not on windows.
				continue;
#endif
			}

			fileFilters.Add(FileFilter.FromKnownType(type));
		}
		if (!filters.Contains(FileType.All))
			fileFilters.Add(FileFilter.FromKnownType(FileType.All));

		Filters = fileFilters;
	}
}
