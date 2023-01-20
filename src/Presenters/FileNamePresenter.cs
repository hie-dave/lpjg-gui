using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Views;
using System.Reflection;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// This class presents a file name input to the user.
/// </summary>
public class FileNamePresenter : PropertyPresenterBase<string?>
{
	/// <summary>
	/// The view.
	/// </summary>
	private readonly IPropertyView view;

	/// <summary>
	/// Create a new <see cref="FileNamePresenter"/> instance for the specified
	/// model and property.
	/// </summary>
	/// <param name="model">The model.</param>
	/// <param name="property">A property of the model.</param>
	/// <param name="attribute">The file name attribute on the property.</param>
	public FileNamePresenter(object model, PropertyInfo property, FileNameAttribute attribute)
		: base(model, property)
	{
		// Note that the generic type parameter is nullable string. So a null
		// value is allowed here.
		string? initial = property.GetValue(model) as string ?? "";
		view = new FileNameView(attribute.Name, GetDescription(), initial, OnChanged);
	}

	/// <inheritdoc />
	public override bool CanPresent(PropertyInfo property)
	{
		if (!base.CanPresent(property))
			return false;
		return property.GetCustomAttribute<FileNameAttribute>() != null;
	}

	/// <inheritdoc />
	public override void Dispose()
	{
		view.Dispose();
	}

	/// <inheritdoc />
	public override IPropertyView GetView() => view;

	/// <summary>
	/// Called when the property value is changed by the user.
	/// </summary>
	/// <param name="newValue">New value of the property.</param>
	private void OnChanged(string newValue)
	{
		try
		{
			string? value = newValue.Trim();

			// If property is a nullable string, and user has set the value to
			// an empty string, use null instead.
			if (string.IsNullOrWhiteSpace(newValue) && Nullable.GetUnderlyingType(property.PropertyType) != null)
				value = null;
			property.SetValue(model, value);
		}
		catch (Exception error)
		{
			throw new Exception($"Unable to set property {property.Name} of class {model.GetType().Name}", error);
		}
	}
}
