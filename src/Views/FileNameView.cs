using Adw;
using Gtk;
using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// This view displays a control for editing a file name property.
/// </summary>
public class FileNameView : EntryRow, IPropertyView
{
	/// <summary>
	/// Name of the property.
	/// </summary>
	private readonly string name;

	/// <summary>
	/// Description of the property.
	/// </summary>
	private readonly string description;

	/// <summary>
	/// Called when the file name is changed.
	/// </summary>
	private readonly Action<string> changedCallback;

	/// <summary>
	/// Create a new <see cref="FileNameView"/> instance.
	/// </summary>
	/// <param name="name">Name of the view.</param>
	/// <param name="description">Description of the view.</param>
	/// <param name="initial">The initial value of the property.</param>
	/// <param name="changedCallback">Function which will be invoked when the user changes the property value.</param>
	/// <returns></returns>
	public FileNameView(string name, string description, string initial, Action<string> changedCallback) : base()
	{
		this.name = name;
		this.description = description;
		this.changedCallback = changedCallback;

		Title = name;
		this.TooltipText = description;

		SetText(initial);

		ShowApplyButton = true;
		OnApply += OnChanged;

		Show();
	}

	/// <inheritdoc />
	public string GetDescription() => description;

	/// <inheritdoc />
	public string PropertyName() => description;

	/// <inheritdoc />
	public Widget GetWidget() => this;

	/// <summary>
	/// Called when the text input is changed.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnChanged(object sender, EventArgs args)
	{
		changedCallback(GetText());
	}
}
