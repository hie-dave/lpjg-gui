using LpjGuess.Core.Models;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// The main presenter type. This doesn't implement IPresenter or PresenterBase,
/// because it doesn't have a model object, so it's not quite a presenter in
/// that sense.
/// </summary>
public class MainPresenter
{
	/// <summary>
	/// Default window title when no files are open.
	/// </summary>
	private const string defaultTitle = "LPJ-Guess";

	/// <summary>
	/// The view object.
	/// </summary>
	private readonly IMainView view;

	/// <summary>
	/// The current child presenter.
	/// </summary>
	private IPresenter<IView> child;

	/// <summary>
	/// Presenter for the preferences dialog.
	/// </summary>
	private PreferencesPresenter? propertiesPresenter;

	/// <summary>
	/// Create a new <see cref="MainPresenter"/> instance connected to the
	/// specified view object.
	/// </summary>
	/// <param name="view"></param>
	public MainPresenter(IMainView view)
	{
		this.view = view;
		view.AddMenuItem("Preferences", OnPreferences);
		view.AddMenuItem("About", OnAbout);
		view.AddMenuItem("Quit", () => view.Close(), "<Ctrl>Q");
		view.OpenFile += OpenFile;

		child = new NoFilePresenter();
		view.SetChild(child.GetView());

		view.SetTitle(defaultTitle);
	}

	/// <summary>
	/// Open the specified file.
	/// </summary>
	/// <param name="file">File path.</param>
	private void OpenFile(string file)
	{
		// Ensure file exists.
		if (!System.IO.File.Exists(file))
			throw new FileNotFoundException($"File not found: '${file}'", file);

		// Close previous file.
		child.Dispose();

		// Open new file.
		LpjFile lpjFile = CreateLpjFile(file);
		child = new FilePresenter(lpjFile);
		view.SetChild(child.GetView());

		// Update window title.
		view.SetTitle(Path.GetFileName(file), Path.GetDirectoryName(file));
	}

	private LpjFile CreateLpjFile(string file)
	{
		if (Path.GetExtension(file).ToLower() == ".ins")
		{
			return LpjFile.ForInsFile(file);
		}
		return LpjFile.LoadFrom(file);
	}

	/// <summary>
	/// User has selected the "Preferences" menu item.
	/// </summary>
	private void OnPreferences()
	{
		try
		{
			propertiesPresenter = new PreferencesPresenter(Configuration.Instance, OnPreferencesClosed);
			propertiesPresenter.Show();
		}
		catch (Exception error)
		{
			view.ReportError(error);
		}
	}

	/// <summary>
	/// User has closed the preferences dialog. Save preferences to disk.
	/// </summary>
	private void OnPreferencesClosed()
	{
		Configuration.Instance.Save();
		propertiesPresenter?.Dispose();
		propertiesPresenter = null;

		if (child is FilePresenter fp)
			fp.PopulateRunners();
	}

	/// <summary>
	/// User has selected the "About" menu item.
	/// </summary>
	private void OnAbout()
	{
		try
		{
			new AboutView(view).Show();
		}
		catch (Exception error)
		{
			view.ReportError(error);
		}
	}
}
