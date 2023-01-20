using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Views;
using System.Reflection;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a view which displays properties of an object for a user to
/// modify.
/// </summary>
public class PropertiesPresenter : IPresenter<IPropertiesView>
{
	/// <summary>
	/// The view.
	/// </summary>
	private readonly IPropertiesView view;

	/// <summary>
	/// The presenters for each property.
	/// </summary>
	private readonly IReadOnlyList<IPropertyPresenter> presenters;

	/// <summary>
	/// An error handler function.
	/// </summary>
	private readonly Action<Exception> errorHandler;

	/// <summary>
	/// Action to be invoked when the properties GUI is closed.
	/// </summary>
	private readonly Action onClose;

	/// <summary>
	/// Create a new <see cref="PropertiesPresenter"/> instance for the
	/// specified model.
	/// </summary>
	/// <param name="mainView">The main view (required in order to pass window reference).</param>
	/// <param name="model">The model.</param>
	/// <param name="onClose">Action to be invoked when the properties GUI is closed.</param>
	public PropertiesPresenter(IMainView mainView, object model, Action onClose)
	{
		this.onClose = onClose;
		this.errorHandler = mainView.ReportError;
		presenters = Parse(model);
		view = new PropertiesDialog(mainView, presenters.Select(p => p.GetView()).ToList(), errorHandler, OnClose);
	}

	/// <summary>
	/// Show the properties presenter.
	/// </summary>
	public void Show()
	{
		view.Show();
	}

	/// <inheritdoc />
	public IPropertiesView GetView() => view;

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public void Dispose()
	{
		view.Dispose();
	}

	/// <summary>
	/// Create a list of property presenters for each presentable property of an
	/// object.
	/// </summary>
	/// <param name="model">The model object (may be of any type).</param>
	private IReadOnlyList<IPropertyPresenter> Parse(object model)
	{
		const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
		Type type = model.GetType();
		List<IPropertyPresenter> presenters = new List<IPropertyPresenter>();
		foreach (PropertyInfo property in type.GetProperties(flags))
		{
			if ( !(property.CanRead && property.CanWrite) )
				continue;
			UIAttribute? attribute = property.GetCustomAttribute<UIAttribute>();
			if (attribute == null)
				continue;

			// This is a user-configurable property.
			presenters.Add(CreatePropertyPresenter(model, property, attribute, errorHandler));
		}
		return presenters;
	}

	/// <summary>
	/// Create a property presenter for the specified property.
	/// </summary>
	/// <param name="model">The model.</param>
	/// <param name="property">The property.</param>
	/// <param name="attribute">The UI attribute on the property.</param>
	/// <param name="errorCallback">An error handling function to be used by the views.</param>
	private IPropertyPresenter CreatePropertyPresenter(object model, PropertyInfo property
		, UIAttribute attribute, Action<Exception> errorCallback)
	{
		if (property.PropertyType == typeof(bool))
		{
			return new BoolPropertyPresenter(model, property, attribute, errorCallback);
		}
		else if (property.PropertyType == typeof(string))
		{
			if (attribute is FileNameAttribute fileAttribute)
				return new FileNamePresenter(model, property, fileAttribute);
		}
		throw new InvalidOperationException($"No known property presenter supports property {property.Name} of object type {model.GetType().Name}");
	}

	/// <summary>
	/// Called by the view when it is closed.
	/// </summary>
	private void OnClose()
	{
		onClose();
	}
}
