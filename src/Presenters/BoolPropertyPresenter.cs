using System.Reflection;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// Presenter for a boolean property.
/// </summary>
public class BoolPropertyPresenter : PropertyPresenterBase<bool>
{
	/// <summary>
	/// The property view managed by this presenter.
	/// </summary>
	protected readonly IPropertyView view;

	/// <summary>
	/// Create a new <see cref="BoolPropertyPresenter"/> instance.
	/// </summary>
	/// <param name="model">The model.</param>
	/// <param name="property">The property to be presented..</param>
	/// <param name="attribute">The property's UI attribute.</param>
	/// <param name="errorCallback">An error handling routine for the view.</param>
	public BoolPropertyPresenter(object model, PropertyInfo property, UIAttribute attribute, Action<Exception> errorCallback)
		: base(model, property)
	{
		object? value = property.GetValue(model);
		if (value == null)
			throw new InvalidOperationException($"Unable to read value of property {property.Name} of class {model.GetType().Name}");

		bool initial = (bool)value;
		string desc = GetDescription();
		view = new BoolPropertyView(attribute.Name, desc, initial, OnChanged, errorCallback);
	}

	/// <summary>
	/// Dispose of unmanaged resources.
	/// </summary>
	public override void Dispose()
	{
		view.Dispose();
	}

	/// <inheritdoc />
	public override IPropertyView GetView() => view;

	/// <summary>
	/// Called when the user changes the property value.
	/// </summary>
	/// <param name="newValue">New value of the property.</param>
	private void OnChanged(bool newValue)
	{
		property.SetValue(model, newValue);
	}
}
