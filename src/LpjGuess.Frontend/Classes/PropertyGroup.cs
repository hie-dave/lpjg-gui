using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Classes;

/// <summary>
/// A collection of properties to be grouped together inside of a single
/// <see cref="PropertyPage"/> when displayed in the GUI.
/// </summary>
public class PropertyGroup : IPropertyGroup
{
	/// <inheritdoc />
	public string? Name { get; private init; }

	/// <inheritdoc />
	public IEnumerable<IPropertyPresenter> Presenters { get; private init; }

	/// <summary>
	/// Create a new <see cref="PropertyGroup"/> instance.
	/// </summary>
	/// <param name="name">Name of the group.</param>
	/// <param name="presenters">Properties in this group.</param>
	public PropertyGroup(string? name, IEnumerable<IPropertyPresenter> presenters)
	{
		Name = name;
		Presenters = presenters;
	}
}
