namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface to a page of properties to be displayed in the GUI.
/// </summary>
public interface IPropertyPage
{
	/// <summary>
	/// Name of the property page.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Property groups to be displayed on this page.
	/// </summary>
	IEnumerable<IPropertyGroup> Groups { get; }
}
