using System.Reflection;
using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// Common functionality for property presenters.
/// </summary>
/// <typeparam name="T">The type of property being presented by this presenter.</typeparam>
public abstract class PropertyPresenterBase<T> : IPropertyPresenter
{
	/// <summary>
	/// The model object.
	/// </summary>
	protected readonly object model;

	/// <summary>
	/// The property being presented.
	/// </summary>
	protected readonly PropertyInfo property;

	/// <summary>
	/// Create a new <see cref="PropertyPresenterBase{T}"/> instance.
	/// </summary>
	protected PropertyPresenterBase(object model, PropertyInfo property)
	{
		if (!model.GetType().IsAssignableFrom(property.DeclaringType))
			throw new InvalidOperationException($"Model type ({model.GetType().Name}) does not match proprty type ({property.PropertyType.Name})");
		if (!CanPresent(property))
			throw new InvalidOperationException($"BoolPropertyPresenter cannot present property {property.PropertyType.Name}");

		this.model = model;
		this.property = property;
	}

	/// <inheritdoc />
	public virtual bool CanPresent(PropertyInfo property)
	{
		return property.PropertyType == typeof(T);
	}

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public abstract void Dispose();

	/// <inheritdoc />
	public abstract IPropertyView GetView();

	/// <summary>
	/// Get a description of the specified property.
	/// </summary>>
	protected string GetDescription() => throw new NotImplementedException(); // property.GetSummary();

	/// <inheritdoc />
	public object GetModel() => model;
}
