using Adw;
using Gtk;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays a collection property. That is, a property which is an
/// enumerable type.
/// </summary>
public class CollectionPropertyView : PreferencesPage, ICollectionPropertyView
{
	/// <summary>
	/// Name of the collection.
	/// </summary>
	private readonly string name;

	/// <summary>
	/// Description of the collection.
	/// </summary>
	private readonly string description;

	/// <summary>
	/// Name of the 'type' each element, used to populate the "add item" button.
	/// E.g. "elem" -> "Add elem".
	/// </summary>
	private readonly string elementName;

	/// <summary>
	/// The property groups displayed in the UI.
	/// </summary>
	private IEnumerable<IPropertyGroup> groups;

	/// <summary>
	/// Function to be invoked when the user wants to add an element to the collection.
	/// </summary>
	private readonly Action addItem;

	/// <summary>
	/// Function to be invoked when the user wants to delete an element of the collection.
	/// </summary>
	private readonly Action<IPropertyGroup> deleteItem;

	/// <summary>
	/// A lookup table mapping preference groups to property groups.
	/// </summary>
	private readonly IDictionary<PreferencesGroup, IPropertyGroup> groupLookup;

	/// <summary>
	/// Create a new <see cref="CollectionPropertyView"/> instance.
	/// </summary>
	/// <param name="name">Name of the collection.</param>
	/// <param name="description">Description of the collection.</param>
	/// <param name="elementName">Name of the 'type' each element, used to populate the "add item" button. E.g. "elem" -> "Add elem".</param>
	/// <param name="properties">The views for each property in the collection.</param>
	/// <param name="onAddItem">Function to be invoked when the user wants to add an element to the collection.</param>
	/// <param name="onDeleteItem">Function to be invoked when the user wants to delete an element from the collection.</param>
	public CollectionPropertyView(string name, string description, string elementName, IEnumerable<IPropertyGroup> properties
		, Action onAddItem, Action<IPropertyGroup> onDeleteItem)
	{
		this.name = name;
		this.description = description;
		this.elementName = elementName;
		this.groups = properties;
		this.addItem = onAddItem;
		this.deleteItem = onDeleteItem;

		Title = name;
		IconName = Icons.Settings;

		groupLookup = new Dictionary<PreferencesGroup, IPropertyGroup>();

		Populate();
	}

	/// <summary>
	/// Populate the view.
	/// </summary>
	private void Populate()
	{
		foreach (IPropertyGroup group in groups)
			AddGroup(group);

		// Add an extra row which contains an 'add item' button.
		ActionRow addItemRow = new AddElementRow($"Add {elementName}", addItem);
		PreferencesGroup addItemGroup = new PreferencesGroup();
		addItemGroup.Add(addItemRow);
		Add(addItemGroup);
	}

	/// <inheritdoc />
	public void AddGroup(IPropertyGroup group)
	{
		IEnumerable<IPropertyView> views = group.Presenters.Select(p => p.GetView());
		PreferencesGroup view = new DeletablePropertiesView(group.Name, views, OnDeleteGroup);
		groupLookup[view] = group;
		Add(view);
	}

	/// <summary>
	/// Called when the user wants to delete an element of the collection.
	/// </summary>
	/// <param name="deletee">The group to be deleted.</param>
	private void OnDeleteGroup(PreferencesGroup deletee)
	{
		if (groupLookup.ContainsKey(deletee))
		{
			IPropertyGroup group = groupLookup[deletee];
			groupLookup.Remove(deletee);
			Remove(deletee);
			deletee.Dispose();
			deleteItem(group);
		}
	}

	/// <inheritdoc />
	public string GetDescription() => description;

	/// <inheritdoc />
	public string PropertyName() => name;

	/// <inheritdoc />
	Widget IView.GetWidget() => this;
}
