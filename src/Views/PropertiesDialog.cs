using Adw;
using Gtk;
using LpjGuess.Frontend.Interfaces;
using ApplicationWindow = Adw.ApplicationWindow;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A window which displays properties to the user.
/// </summary>
public class PropertiesDialog : PreferencesWindow, IPropertiesView
{
	/// <summary>
	/// A properties view which will render the properties. The IPropertiesView
	/// API is implemented by proxying requests to/from this object.
	/// </summary>
	private readonly PropertiesView propertiesView;

	/// <summary>
	/// Error handler routine.
	/// </summary>
	private readonly Action<Exception> errorHandler;

	/// <summary>
	/// Action to be invoked when the dialog is closed.
	/// </summary>
	private readonly Action onClosed;

	/// <summary>
	/// Create a new <see cref="PropertiesDialog"/> instance.
	/// </summary>
	/// <param name="mainView">The main view (a reference is required to set the parent window).</param>
	/// <param name="properties">The propreties to be displayed.</param>
	/// <param name="errorHandler">Error handler routine.</param>
	/// <param name="onClosed">Action to be invoked when the dialog is closed.</param>
	public PropertiesDialog(IMainView mainView, IReadOnlyList<IPropertyView> properties, Action<Exception> errorHandler, Action onClosed)
	{
		if ( !(mainView.GetWidget() is ApplicationWindow parent) )
			throw new InvalidOperationException($"Programming error: main view is not a window");

		this.errorHandler = errorHandler;
		this.onClosed = onClosed;

		Modal = true;
		TransientFor = parent;
		Title = "Preferences";
		HideOnClose = true;

		this.OnCloseRequest += OnClosed;

		// PropertiesView uses PreferenceGroup. This gets added to a preferences
		// page, which gets added to the preferences window.
		propertiesView = new PropertiesView(properties);
		PreferencesPage page = new PreferencesPage();
		page.Name = "General"; // fixme
		page.Add(propertiesView);
		Add(page);

		Hide();
	}

	/// <inheritdoc />
	void IPropertiesView.Show()
	{
		Present();
	}

	/// <inheritdoc />
	Widget IView.GetWidget() => this;
	
	/// <summary>
	/// Dispose of native resources.
	/// </summary>
	public override void Dispose()
	{
		base.Dispose();
	}

	/// <summary>
	/// Called when the dialog is closed.
	/// </summary>
	/// <param name="sender">Sender object..</param>
	/// <param name="args">Event data.</param>
	private void OnClosed(object sender, EventArgs args)
	{
		try
		{
			onClosed();
		}
		catch (Exception error)
		{
			errorHandler(error);
		}
	}
}
