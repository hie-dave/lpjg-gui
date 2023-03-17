using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Classes;

/// <summary>
/// A page of properties to be displayed in the GUI.
/// </summary>
public class PropertyPage : IPropertyPage
{
	/// <summary>
	/// Name of the property page.
	/// </summary>
	public string Name { get; private init; }

	/// <summary>
	/// Property groups to be displayed on this page.
	/// </summary>
	public IEnumerable<IPropertyGroup> Groups { get; private init; }

	/// <summary>
	/// Create a new <see cref="PropertyPage"/> instance.
	/// </summary>
	/// <param name="name">Name of the page.</param>
	/// <param name="groups">Property groups to be displayed.</param>
	public PropertyPage(string name, IEnumerable<IPropertyGroup> groups)
	{
		Name = name;
		Groups = groups;
	}
}
