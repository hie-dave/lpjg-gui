using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// Presenter for a <see cref="RecentFilesView"/>. This view doesn't do anything
/// (it's a placeholder), so this presenter doesn't really do anything either.
/// </summary>
public class RecentFilesPresenter : PresenterBase<RecentFilesView>
{
	/// <summary>
	/// Create a new <see cref="RecentFilesPresenter"/> instance.
	/// </summary>
	public RecentFilesPresenter() : base(new RecentFilesView())
	{
		view.Populate(Configuration.Instance.RecentWorkspaces.Where(File.Exists));
		OnOpenFile = new Event<string>();
		view.OnClick.ConnectTo(OnOpenFile);
	}

	/// <summary>
	/// Invoked when the user wants to open one of the recent files.
	/// </summary>
	public Event<string> OnOpenFile { get; private init; }

	/// <inheritdoc /> 
    public override void Dispose()
    {
		OnOpenFile.Dispose();
        base.Dispose();
    }
}
