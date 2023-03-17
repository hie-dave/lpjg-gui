namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface for a view which displays a collection property. That is, a
/// property which is an enumerable type.
/// </summary>
public interface ICollectionPropertyView : IPropertyView
{
	/// <summary>
	/// Add a property group to the page.
	/// </summary>
	/// <param name="group">The property group.</param>
	void AddGroup(IPropertyGroup group);
}
