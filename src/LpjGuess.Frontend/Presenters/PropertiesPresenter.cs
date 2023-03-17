// using ModelBinding.Attributes;
// using LpjGuess.Frontend.Classes;
// using LpjGuess.Frontend.Interfaces;
// using LpjGuess.Frontend.Views;
// using System.Collections;
// using System.Reflection;

// namespace LpjGuess.Frontend.Presenters;

// /// <summary>
// /// A presenter for a view which displays properties of an object for a user to
// /// modify.
// /// </summary>
// public class PropertiesPresenter : IPresenter<IPropertiesView>
// {
// 	/// <summary>
// 	/// The view.
// 	/// </summary>
// 	private readonly IPropertiesView view;

// 	/// <summary>
// 	/// The presenters for each property.
// 	/// </summary>
// 	private readonly IReadOnlyList<IPropertyPage> pages;

// 	/// <summary>
// 	/// Action to be invoked when the properties GUI is closed.
// 	/// </summary>
// 	private readonly Action onClose;

// 	/// <summary>
// 	/// Create a new <see cref="PropertiesPresenter"/> instance for the
// 	/// specified model.
// 	/// </summary>
// 	/// <param name="mainView">The main view (required in order to pass window reference).</param>
// 	/// <param name="model">The model.</param>
// 	/// <param name="onClose">Action to be invoked when the properties GUI is closed.</param>
// 	public PropertiesPresenter(IMainView mainView, object model, Action onClose)
// 	{
// 		this.onClose = onClose;
// 		this.errorHandler = mainView.ReportError;
// 		pages = Parse(model);
// 		view = new PropertiesDialog(mainView, pages, errorHandler, OnClose);
// 	}

// 	/// <summary>
// 	/// Show the properties presenter.
// 	/// </summary>
// 	public void Show()
// 	{
// 		view.Show();
// 	}

// 	/// <inheritdoc />
// 	public IPropertiesView GetView() => view;

// 	/// <summary>
// 	/// Dispose of native resources.
// 	/// </summary>
// 	public void Dispose()
// 	{
// 		view.Dispose();
// 	}

// 	/// <summary>
// 	/// Create a list of property presenters for each presentable property of an
// 	/// object.
// 	/// </summary>
// 	/// <param name="model">The model object (may be of any type).</param>
// 	private IReadOnlyList<IPropertyPage> Parse(object model)
// 	{
// 		const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
// 		Type type = model.GetType();

// 		List<IPropertyPage> pages = new List<IPropertyPage>();

// 		List<IPropertyPresenter> presenters = new List<IPropertyPresenter>();
// 		foreach (PropertyInfo property in type.GetProperties(flags))
// 		{
// 			if ( !(property.CanRead && property.CanWrite) )
// 				continue;
// 			UIAttribute? attribute = property.GetCustomAttribute<UIAttribute>();
// 			if (attribute == null)
// 				continue;

// 			if (property.PropertyType != typeof(string) && 
// 				typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
// 			{
// 				pages.Add(ListPage(model, property));
// 				continue;
// 			}

// 			if (property.PropertyType.IsClass)
// 			{
// 				object? value = property.GetValue(model);
// 				if (value != null)
// 					pages.AddRange(Parse(value));
// 			}

// 			// This is a user-configurable property.
// 			presenters.Add(CreatePropertyPresenter(model, property, attribute, errorHandler));
// 		}

// 		// General configuration on first page.
// 		pages.Insert(0, new PropertyPage("General", new[] { new PropertyGroup(null, presenters) }));

// 		return pages;
// 	}

// 	private IPropertyPage ListPage(object model, PropertyInfo property)
// 	{
// 		if (!typeof(IEnumerable<object>).IsAssignableFrom(property.PropertyType))
// 			throw new InvalidOperationException("ListPage() called for non-IEnumerable property. This is a programming error, please report this");

// 		UIAttribute? attrib = property.GetCustomAttribute<UIAttribute>();
// 		if (attrib == null)
// 			throw new InvalidOperationException($"Property {property.Name} of class {model.GetType().Name} does not contain a UI attribute");

// 		string pageName = attrib.Name;

// 		IEnumerable<object>? values = property.GetValue(model) as IEnumerable<object>;
// 		List<IPropertyGroup> groups = new List<IPropertyGroup>();
// 		foreach (object value in values ?? Enumerable.Empty<object>())
// 		{
// 			IReadOnlyList<IPropertyPage> pages = Parse(value);
// 			try
// 			{
// 				string ename = value.GetType().Name;
// 				if (pages.Count == 0)
// 					throw new InvalidOperationException($"Element {ename} contains 0 user-settable properties");
// 				if (pages.Count > 1)
// 					throw new InvalidOperationException($"Element {ename} contains multiple pages of properties (ie nested list)");
// 				IPropertyPage page = pages[0];
// 				int ngroup = page.Groups.Count();
// 				if (ngroup == 0)
// 					throw new InvalidOperationException($"Element {ename} contains 0 user-settable properties");
// 				if (ngroup > 1)
// 					throw new InvalidOperationException($"Element {ename} contains multiple groups of properties (ie list)");
// 				groups.Add(page.Groups.First());
// 			}
// 			catch (Exception error)
// 			{
// 				string pname = property.Name;
// 				string mname = model.GetType().Name;
// 				throw new InvalidOperationException($"Property {pname} on model {mname} is not a presentable property", error);
// 			}
// 		}
// 		// groups.Add(new PropertyGroup(null, new[] { new AddElementPresenter(model, property) }));
// 		return new CollectionPropertyPresenter(model, property, groups);
// 	}

// 	/// <summary>
// 	/// Create a property presenter for the specified property.
// 	/// </summary>
// 	/// <param name="model">The model.</param>
// 	/// <param name="property">The property.</param>
// 	/// <param name="attribute">The UI attribute on the property.</param>
// 	/// <param name="errorCallback">An error handling function to be used by the views.</param>
// 	private static IPropertyPresenter CreatePropertyPresenter(object model, PropertyInfo property, UIAttribute attribute)
// 	{
// 		if (property.PropertyType == typeof(bool))
// 		{
// 			return new BoolPropertyRenderer(model, property, attribute);
// 		}
// 		else if (property.PropertyType == typeof(string))
// 		{
// 			if (attribute is FileNameAttribute fileAttribute)
// 				return new FileNamePresenter(model, property, fileAttribute);
// 		}
// 		throw new InvalidOperationException($"No known property presenter supports property {property.Name} of object type {model.GetType().Name}");
// 	}

// 	/// <summary>
// 	/// Called by the view when it is closed.
// 	/// </summary>
// 	private void OnClose()
// 	{
// 		onClose();
// 	}
// }
