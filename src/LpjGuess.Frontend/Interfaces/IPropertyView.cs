namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface for a view which is a widget used to control the value of the
/// property of an object.
/// </summary>
public interface IPropertyView : IView
{
	/// <summary>
	/// Get a human-readable name of the property.
	/// </summary>
	public string PropertyName();

	/// <summary>
	/// Get a description of the property.
	/// </summary>
	public string GetDescription();
}
