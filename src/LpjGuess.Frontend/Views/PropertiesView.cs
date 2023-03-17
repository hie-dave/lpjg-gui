using Gtk;
using LpjGuess.Frontend.Interfaces;
#if !WINDOWS
using Adw;
#endif

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which lays out multiple properties for the user to edit.
/// </summary>
public class PropertiesView : PreferencesGroup, IPropertiesView
{
	/// <summary>
	/// The properties.
	/// </summary>
	private readonly IEnumerable<IPropertyView> properties;

	/// <summary>
	/// Create a new <see cref="PropertiesView"/> instance which will lay out
	/// the individual property views passed in. Note that these property views
	/// are not owned by this object, and must be disposed of by the caller.
	/// </summary>
	/// <param name="name">Name of the collection of properties.</param>
	/// <param name="properties"></param>
	public PropertiesView(string? name, IEnumerable<IPropertyView> properties)
	{
		if (name != null)
			this.Title = name;
		this.properties = properties;

		foreach (IPropertyView view in properties)
		{
// #if WINDOWS
			// Box box = new Box();
			// box.Orientation = Orientation.Horizontal;
			// box.Append(Label.New(properties[i].PropertyName()));
			// box.Append(properties[i].GetWidget());
			// box.TooltipText = properties[i].GetDescription();
			Add(view.GetWidget());
// #else
// 			Append(properties[i].GetWidget());
// #endif
		}
		Show();
	}

	/// <inheritdoc />
	Widget IView.GetWidget() => this;

	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public override void Dispose()
	{
		// First, remove the child property views; we don't own them so we
		// cannot dispose of them here.
		foreach (IPropertyView child in properties)
			Remove(child.GetWidget());

		base.Dispose();
	}
}
