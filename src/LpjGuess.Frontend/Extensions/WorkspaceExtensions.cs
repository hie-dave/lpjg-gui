using LpjGuess.Core.Models;
using LpjGuess.Frontend.Serialisation.Json;

namespace LpjGuess.Frontend.Extensions;

/// <summary>
/// Extension methods for <see cref="Workspace"/>.
/// </summary>
public static class WorkspaceExtensions
{
	/// <summary>
	/// Save all changes to this file to disk.
	/// </summary>
	public static void Save(this Workspace workspace)
	{
		workspace.SerialiseTo(workspace.FilePath);
	}

	/// <summary>
	/// Deserialise the specified file into an instance of <see cref="Workspace"/>.
	/// </summary>
	/// <param name="filePath">Path to the serialised file.</param>
	public static Workspace LoadWorkspace(this string filePath)
	{
		return JsonSerialisation.DeserialiseFrom<Workspace>(filePath);
	}
}
