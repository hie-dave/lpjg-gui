namespace LpjGuess.Frontend.Attributes;

/// <summary>
/// This attribute, when applied to a property, will cause the property to be
/// displayed in the GUI and controls how the property is displayed.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class UIAttribute : Attribute
{
	/// <summary>
	/// Name of the property as it will be displayed in the GUI.
	/// </summary>
	public string Name { get; private init; }

	/// <summary>
	/// Create a new <see cref="UIAttribute"/> instance.
	/// </summary>
	/// <param name="name">Name of the attribute as it wil be displayed in the GUI.</param>
	public UIAttribute(string name)
	{
		Name = name;
	}
}
