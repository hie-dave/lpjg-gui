using Adw;
using Gtk;
using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// Class which can display a boolean property for editing.
/// </summary>
public class BoolPropertyView : ActionRow, IPropertyView
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
	/// Callback to be invoked when the property value is changed.
	/// </summary>
	private readonly Action<bool> onChanged;

	/// <summary>
	/// Error handling function.
	/// </summary>
	private readonly Action<Exception> errorCallback;

	/// <summary>
	/// The internal toggle button widget.
	/// </summary>
	private readonly Switch button;

	/// <summary>
	/// Create a new <see cref="BoolPropertyView"/> instance/
	/// </summary>
	/// <param name="name">Name of the property.</param>
	/// <param name="description">Description of the property.</param>
	/// <param name="initial">Initial value of the property.</param>
	/// <param name="changedCallback">Callback to be invoked when the property value is changed.</param>
	/// <param name="errorCallback">Error handling function.</param>
	public BoolPropertyView(string name, string description, bool initial,
		Action<bool> changedCallback, Action<Exception> errorCallback) : base()
	{
		this.name = name;
		this.description = description;
		this.onChanged = changedCallback;
		this.errorCallback = errorCallback;

		button = new Switch();
		button.Active = initial;
		button.OnStateSet += OnToggled;
		button.Valign = Align.Center;

		Title = name;
		Subtitle = description;

		AddSuffix(button);
		Show();
	}

	/// <inheritdoc />
	public string GetDescription() => description;

	/// <inheritdoc />
	public string PropertyName() => name;

	/// <inheritdoc />
	Widget IView.GetWidget() => this;

	/// <summary>
	/// Called when the user toggles the property value.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnToggled(object sender, EventArgs args)
	{
		try
		{
			onChanged(button.Active);
		}
		catch (Exception error)
		{
			errorCallback(error);
		}
	}
}
