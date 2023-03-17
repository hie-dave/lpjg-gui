using Adw;
using Gtk;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Utility.Gtk;
using ApplicationWindow = Adw.ApplicationWindow;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A window which displays properties to the user.
/// </summary>
public class PropertiesDialog : PreferencesWindow, IPropertiesView
{
	/// <summary>
	/// Action to be invoked when the dialog is closed.
	/// </summary>
	private readonly Action onClosed;

	/// <summary>
	/// The preferences pages in this window.
	/// </summary>
	private readonly List<PreferencesPage> pages;

	/// <summary>
	/// Create a new <see cref="PropertiesDialog"/> instance.
	/// </summary>
	/// <param name="mainView">The main view (a reference is required to set the parent window).</param>
	/// <param name="properties">The propreties to be displayed.</param>
	/// <param name="onClosed">Action to be invoked when the dialog is closed.</param>
	public PropertiesDialog(IMainView mainView, IReadOnlyList<IPropertyPage> properties, Action onClosed)
	{
		if ( !(mainView.GetWidget() is ApplicationWindow parent) )
			throw new InvalidOperationException($"Programming error: main view is not a window");

		this.onClosed = onClosed;

		Modal = true;
		TransientFor = parent;
		Title = "Preferences";
		HideOnClose = true;

		this.OnCloseRequest += OnClosed;
		pages = new List<PreferencesPage>();

		Populate(properties);
		Hide();
	}

	/// <summary>
	/// Populate the properties dialog with properties.
	/// </summary>
	/// <param name="properties">The properties.</param>
	public void Populate(IEnumerable<IPropertyPage> properties)
	{
		string? currentPage = VisiblePageName;

		foreach (PreferencesPage page in pages)
		{
			Remove(page);
			page.Dispose();
		}
		pages.Clear();

		// PropertiesView uses PreferenceGroup. This gets added to a preferences
		// page, which gets added to the preferences window.
		foreach (IPropertyPage propertyPage in properties)
		{
			// please fix this
			if (propertyPage is IPropertyPresenter presenter && presenter.GetView() is PreferencesPage page)
			{
				Add(page);
				pages.Add(page);
			}
			else
			{
				PreferencesPage preferencesPage = new PreferencesPage();
				preferencesPage.Title = propertyPage.Name;
				preferencesPage.IconName = Icons.Settings;
				foreach (IPropertyGroup group in propertyPage.Groups)
				{
					IEnumerable<IPropertyView> views = group.Presenters.Select(p => p.GetView());
					PreferencesGroup view = new PropertiesView(group.Name, views);
					preferencesPage.Add(view);
				}
				Add(preferencesPage);
				pages.Add(preferencesPage);
			}

		}

		if (currentPage != null)
			VisiblePageName = currentPage;
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
			MainView.Instance.ReportError(error);
		}
	}
}
