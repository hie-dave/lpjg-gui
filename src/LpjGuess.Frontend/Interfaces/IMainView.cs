using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface for the main view.
/// </summary>
public interface IMainView : IView
{
	/// <summary>
	/// Called when the user wants to create a new workspace. The parameter is
	/// the path to the workspace the user wants to create.
	/// </summary>
	Event<string> OnNew { get; }

	/// <summary>
	/// Called when the user wants to open an existing workspace. The parameter
	/// is the path to the workspace the user wants to open.
	/// </summary>
	Event<string> OnOpen { get; }

	/// <summary>
	/// Called when the user wants to create a new workspace from an instruction
	/// file. The parameter is the path to the instruction file.
	/// </summary>
	Event<string> OnNewFromInstructionFile { get; }

	/// <summary>
	/// Add a menu item with the specified name and optional keyboard shortcut
	/// to the menu in the window title bar.
	/// </summary>
	/// <param name="name">Name of the menu item.</param>
	/// <param name="callback">Event callback delegate.</param>
	/// <param name="hotkey">Optional keyboard shortcut. E.g. "&lt;Ctrl&gt;T".</param>
	/// <remarks>
	/// The format for keyboard shortcuts looks like "&lt;Control&gt;a" or
	/// "&lt;Shift&gt;&lt;Alt&gt;F1". The parser is not case-sensitive, and is liberal
	/// enough to accept abbreviations such as "&lt;Ctrl&gt;" and "&lt;Ctl&gt;".
	/// 
	/// Keyboard shortcut syntax is described here:
	/// 
	/// https://docs.gtk.org/gtk4/func.accelerator_parse.html
	/// </remarks>
	void AddMenuItem(string name, Action callback, string? hotkey = null);

	/// <summary>
	/// Close the window.
	/// </summary>
	void Close();

	/// <summary>
	/// Report an error to the user. This function will *never* throw.
	/// </summary>
	void ReportError(Exception error);

	/// <summary>
	/// Set the child view within the window.
	/// </summary>
	/// <param name="view">Any view object.</param>
	void SetChild(IView view);

	/// <summary>
	/// Set the window title (and, optionally, subtitle).
	/// </summary>
	/// <param name="title">The new window title.</param>
	/// <param name="subtitle">The (optional) new window subtitle. Will be hidden if null.</param>
	void SetTitle(string title, string? subtitle = null);
}
