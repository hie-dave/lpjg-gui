using System.Reflection;
using Adw;
using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// View which displays an 'about' window.
/// </summary>
public class AboutView : IAboutView
{
	/// <summary>
	/// The internal window widget.
	/// </summary>
	private readonly AboutDialog dialog;

	/// <summary>
	/// The parent window.
	/// </summary>
	private readonly IMainView parent;

	/// <summary>
	/// Github repository URL.
	/// </summary>
	private const string repo = "https://github.com/hie-dave/lpjg-gui";

	/// <summary>
	/// Issue tracker URL.
	/// </summary>
	private const string issueUrl = $"{repo}/issues";

	/// <summary>
	/// Create a new <see cref="AboutView"/> instance.
	/// </summary>
	/// <param name="view">The main window.</param>
	public AboutView(IMainView view)
	{
		parent = view;

		dialog = new AboutDialog();
		dialog.ApplicationName = "LPJ-Guess";
		dialog.DeveloperName = "A graphical frontend for LPJ-Guess.";
		dialog.IssueUrl = issueUrl;
		dialog.Website = repo;
		Version? version = Assembly.GetExecutingAssembly().GetName().Version;
		if (version != null)
			dialog.Version = version.ToString();
		// if (view is MainView main)
		// 	dialog.TransientFor = main;
		ConnectEventHandlers();
		// Other metadata we could set:
		// string[] Artists
		// string? Comments
		// string? Copyright
		// string? DebugInfo
		// string? DebugInfoFilename
		// string[] Designers
		// string[] Developers
		// string[] Documenters
		// string? License
		// string? ReleaseNotes
		// AccessibleRole AccessibleRole
		// string? TranslatorCredits
		// string? ReleaseNotesVersion
		// string? ApplicationIcon
		// License LicenseType
	}

	/// <summary>
	/// Connect all event callbacks.
	/// </summary>
	private void ConnectEventHandlers()
	{
		dialog.OnClosed += OnClosed;
	}

    /// <summary>
    /// Disconnect all event callbacks.
    /// </summary>
    private void DisconnectEventHandlers()
	{
		dialog.OnClosed -= OnClosed;
	}

    private void OnClosed(Dialog sender, EventArgs args)
    {
		DisconnectEventHandlers();
		dialog.Dispose();
    }

	/// <inheritdoc />
	public void Show()
	{
		dialog.Present(parent.GetWidget());
	}
}
