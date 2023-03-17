// using ModelBinding.Attributes;
// using LpjGuess.Frontend.Extensions;
// using LpjGuess.Frontend.Interfaces;
// using ModelBinding.Extensions;
// using LpjGuess.Frontend.Views;
// using LpjGuess.Frontend.Views.Dialogs;
// using System.Collections;
// using System.Reflection;

// namespace LpjGuess.Frontend.Presenters;

// /// <summary>
// /// A property presenter for a collection/list/enumerable property.
// /// </summary>
// public class CollectionPropertyPresenter : IPropertyPresenter, IPropertyPage
// {
// 	/// <summary>
// 	/// The model object.
// 	/// </summary>
// 	private readonly object model;

// 	/// <summary>
// 	/// The model property being presented.
// 	/// </summary>
// 	private readonly PropertyInfo property;

// 	/// <summary>
// 	/// The view.
// 	/// </summary>
// 	private readonly ICollectionPropertyView view;

// 	/// <summary>
// 	/// Name of the properties page.
// 	/// </summary>
// 	public string Name { get; private init; }

// 	/// <summary>
// 	/// Name of each element in the collection.
// 	/// </summary>
// 	private readonly string elementName;

// 	/// <summary>
// 	/// The properties of each element of the collection.
// 	/// </summary>
// 	public IEnumerable<IPropertyGroup> Groups { get; private init; }

// 	/// <summary>
// 	/// Create a new <see cref="CollectionPropertyPresenter"/> instance.
// 	/// </summary>
// 	/// <param name="model">The model object.</param>
// 	/// <param name="property">The model property being presented.</param>
// 	/// <param name="elements">The properties of each element of the collection.</param>
// 	public CollectionPropertyPresenter(object model, PropertyInfo property, IEnumerable<IPropertyGroup> elements)
// 	{
// 		this.model = model;
// 		this.property = property;
// 		this.Groups = elements;

// 		Name = property.GetCustomAttribute<UIAttribute>()?.Name ?? throw new InvalidOperationException($"Property {property.Name} of type {model.GetType().Name} has no UI attribute");
// 		elementName = GetElementType().Name; // fixme

// 		view = new CollectionPropertyView(Name, property.GetSummary(), elementName, elements, errorHandler, OnAddItem, OnDeleteItem);
// 	}

// 	/// <inheritdoc />
// 	public bool CanPresent(PropertyInfo property)
// 	{
// 		return typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
// 			&& !typeof(string).IsAssignableFrom(property.PropertyType);
// 	}

// 	/// <inheritdoc />
// 	public void Dispose() => view.Dispose();

// 	/// <inheritdoc />
// 	public IPropertyView GetView() => view;

// 	/// <inheritdoc />
// 	public object GetModel() => model;

// 	/// <summary>
// 	/// Get the element type of the list.
// 	/// </summary>
// 	private Type GetElementType()
// 	{
// 		if (property.PropertyType.GenericTypeArguments.Length == 0)
// 			throw new InvalidOperationException($"Property type is not generic");
// 		if (property.PropertyType.GenericTypeArguments.Length > 1)
// 			throw new InvalidOperationException($"Property type has multiple generic type parameters");
// 		return property.PropertyType.GenericTypeArguments.First();
// 	}

// 	/// <summary>
// 	/// Called when a new element has been created which may be added to the
// 	/// collection being presented.
// 	/// </summary>
// 	/// <param name="newElement">The new element which has been created.</param>
// 	public void AddToCollection(object newElement)
// 	{
// 		// Get the old property value. The property is guaranteed to be of
// 		// type IEnumerable due to earlier error checks, so if the result of
// 		// the cast is null, it means the property has a null value.
// 		IEnumerable oldValue = property.GetValue(model) as IEnumerable
// 								?? new object[0];

// 		// Append the new value to the existing collection.
// 		IEnumerable newValue = oldValue.Append(newElement);

// 		UpdateModelValue(newValue);
// 	}

// 	/// <summary>
// 	/// Called when the user wants to delete an item from the collection.
// 	/// </summary>
// 	/// <param name="group">Properties of the item to be deleted.</param>
// 	private void OnDeleteItem(IPropertyGroup group)
// 	{
// 		try
// 		{
// 			IPropertyPresenter? first = group.Presenters.FirstOrDefault();
// 			if (first == null)
// 				// Don't think this can happen.
// 				throw new InvalidOperationException($"Object contains no user-settable properties");

// 			object? value = property.GetValue(model);
// 			if (value == null)
// 				// This shouldn't be possible either.
// 				throw new InvalidOperationException($"Collection contains no items");

// 			// This should always work, due to previous error checking.
// 			IEnumerable collection = (IEnumerable)value;
// 			collection = collection.Remove(first.GetModel());

// 			UpdateModelValue(collection);
// 		}
// 		catch (Exception error)
// 		{
// 			string typeName = property.DeclaringType?.Name ?? model.GetType().Name;
// 			throw new Exception($"Unable to delete item from property {property.Name} of class {typeName}", error);
// 		}
// 	}

// 	/// <summary>
// 	/// Update the value stored in the model from a non-generic IEnumerable.
// 	/// </summary>
// 	/// <remarks>
// 	/// This is done by first converting to a generic IEnumerable (if required),
// 	/// and then converting to the correct collection type. The following
// 	/// collection types are currently supported:
// 	/// 
// 	/// - IEnumerable
// 	/// - IEnumerable{T}
// 	/// - IList
// 	/// - IReadOnlyList{T}
// 	/// - List{T}
// 	/// </remarks>
// 	/// <param name="value"></param>
// 	private void UpdateModelValue(IEnumerable value)
// 	{
// 		// Cast the result to a generic IEnumerable (via .Cast<T>()).
// 		// Technically this *could* fail if the property is not a generic
// 		// collection. In practice I don't expect this to happen. If it
// 		// does, an exception will be thrown here.
// 		MethodInfo cast = typeof(Enumerable).GetGenericMethod("Cast", GetElementType());
// 		object result = cast.InvokeNonNullable(null, value);

// 		// Call ToList() if need be.
// 		Type readOnlyListType = typeof(IReadOnlyList<>).MakeGenericType(GetElementType());
// 		if (typeof(IList).IsAssignableFrom(property.PropertyType)
// 		 || readOnlyListType.IsAssignableFrom(property.PropertyType))
// 		{
// 			MethodInfo toList = typeof(Enumerable).GetGenericMethod("ToList", GetElementType());
// 			result = toList.InvokeNonNullable(null, result);
// 		}

// 		// Update the value of the property using the result.
// 		property.SetValue(model, result);
// 	}

// 	/// <summary>
// 	/// Called when the user wants to add an item to the collection.
// 	/// </summary>
// 	/// <remarks>
// 	/// If the element type is an interface, the user will be prompted to select
// 	/// an implementation of this interface. Otherwise, we attempt to create a
// 	/// default instance of the collection's element type. If the element type
// 	/// is a class, this will require the class to have a default constructor.
// 	/// </remarks>
// 	private void OnAddItem()
// 	{
// 		Type elementType = GetElementType();

// 		if (elementType.IsInterface)
// 		{
// 			// Ask the user 
// 			IEnumerable<Type> implementations = elementType.FindImplementations();
// 			AskUserDialog.RunFor(
// 				implementations,
// 				i => i.Name,
// 				$"Select {elementName} type",
// 				"Add",
// 				i => AddToCollection(i.CreateInstance()),
// 				errorHandler
// 			);
// 		}

// 		// This will throw for classes without a default constructor.
// 		AddToCollection(elementType.CreateInstance());
// 	}
// }
