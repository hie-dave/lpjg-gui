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
	private readonly AboutWindow dialog;

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
		dialog = new AboutWindow();
		dialog.ApplicationName = "LPJ-Guess";
		dialog.DeveloperName = "A graphical frontend for LPJ-Guess.";
		dialog.IssueUrl = issueUrl;
		dialog.Website = repo;
		Version? version = Assembly.GetExecutingAssembly().GetName().Version;
		if (version != null)
			dialog.Version = version.ToString();
		if (view is MainView main)
			dialog.TransientFor = main;
		dialog.OnCloseRequest += (_, __) => dialog.Dispose();
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

	/// <inheritdoc />
	public void Show()
	{
		dialog.Present();
	}
}
