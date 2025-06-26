using LpjGuess.Core.Models;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// The main presenter type. This doesn't implement IPresenter or PresenterBase,
/// because it doesn't have a model object, so it's not quite a presenter in
/// that sense.
/// </summary>
public class MainPresenter : PresenterBase<IMainView>, IMainPresenter
{
	/// <summary>
	/// Default window title when no files are open.
	/// </summary>
	private const string defaultTitle = "LPJ-Guess";

	/// <summary>
	/// The presenter factory.
	/// </summary>
	private readonly IPresenterFactory factory;

	/// <summary>
	/// The current child presenter.
	/// </summary>
	private IPresenter<IView> child;

	/// <summary>
	/// Presenter for the preferences dialog.
	/// </summary>
	private IPreferencesPresenter? propertiesPresenter;

	/// <summary>
	/// Create a new <see cref="MainPresenter"/> instance connected to the
	/// specified view object.
	/// </summary>
	/// <param name="view">The view object.</param>
	/// <param name="factory">The presenter factory.</param>
	public MainPresenter(IMainView view, IPresenterFactory factory) : base(view)
	{
		this.factory = factory;
		view.AddMenuItem("Preferences", OnPreferences);
		view.AddMenuItem("About", OnAbout);
		view.AddMenuItem("Quit", OnClose, "<Ctrl>Q");
		view.OnOpen.ConnectTo(OpenFile);
		view.OnNewFromInstructionFile.ConnectTo(OpenFile);
		view.OnNew.ConnectTo(OnNew);
		view.OnClose.ConnectTo(OnClose);

		IRecentFilesPresenter recent = factory.CreatePresenter<IRecentFilesPresenter>();
		recent.OnOpenFile.ConnectTo(OpenFile);
		child = recent;
		view.SetChild(child.GetView());

		view.SetTitle(defaultTitle);
	}

	/// <summary>
	/// Initialise the presenter.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	public async Task InitialiseAsync(CancellationToken ct = default)
	{
		await view.InitialiseAsync(ct);
	}

	/// <summary>
	/// Show the main window.
	/// </summary>
	public void Show()
	{
		view.Show();
	}

	private void OnNew(string path)
	{
		Workspace workspace = new Workspace();
		workspace.FilePath = path;
		workspace.Save();
		OpenWorkspace(workspace);
	}

    /// <summary>
    /// Open the specified file. This can be an instruction file or a workspace.
    /// </summary>
    /// <param name="file">File path.</param>
    private void OpenFile(string file)
	{
		// Ensure file exists.
		if (!File.Exists(file))
			throw new FileNotFoundException($"File not found: '${file}'", file);

		// Close previous file.
		child.Dispose();

		// Open new file.
		Workspace workspace = CreateWorkspace(file);

		OpenWorkspace(workspace);
	}

	private void OpenWorkspace(Workspace workspace)
	{
		// Update recent files list.
		if (!Configuration.Instance.RecentWorkspaces.Contains(workspace.FilePath))
		{
			Configuration.Instance.RecentWorkspaces.Add(workspace.FilePath);
			Configuration.Instance.Save();
		}

		child = new WorkspacePresenter(workspace);
		view.SetChild(child.GetView());

		// Update window title.
		string file = workspace.FilePath;
		view.SetTitle(Path.GetFileName(file), Path.GetDirectoryName(file));
	}

	private Workspace CreateWorkspace(string file)
	{
		if (Path.GetExtension(file).ToLower() == ".ins")
		{
			return Workspace.ForInsFile(file);
		}
		return Workspace.LoadFrom(file);
	}

	/// <summary>
	/// User has selected the "Preferences" menu item.
	/// </summary>
	private void OnPreferences()
	{
		try
		{
			propertiesPresenter = factory.CreatePresenter<IPreferencesPresenter>();
			propertiesPresenter.OnClosed.ConnectTo(OnPreferencesClosed);
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

		if (child is WorkspacePresenter fp)
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

	/// <summary>
	/// Called when the user wants to close the window.
	/// </summary>
    private void OnClose()
    {
		try
        {
			if (child != null)
			{
				child.Dispose();
			}
			view.Close();
		}
		catch (Exception error)
		{
			view.ReportError(error);
		}
    }
}
