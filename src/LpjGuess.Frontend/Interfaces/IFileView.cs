using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface for a view which may display an instruction file to the user.
/// </summary>
public interface IFileView : IView
{
	/// <summary>
	/// Currently-selected input module.
	/// </summary>
	string InputModule { get; }

	/// <summary>
	/// Display stdout received from the guess child process to the user.
	/// </summary>
	/// <param name="stdout">Text written to stdout by lpj-guess.</param>
	void AppendOutput(string stdout);

	/// <summary>
	/// Display stderr received from the guess child process to the user.
	/// </summary>
	/// <param name="stderr">Text written to stderr by lpj-guess.</param>
	void AppendError(string stderr);

	/// <summary>
	/// Clear the buffer of messages received from lpj-guess.
	/// </summary>
	void ClearOutput();

	/// <summary>
	/// Toggle the visibility of the run/stop buttons.
	/// </summary>
	/// <param name="show">
	/// If true, the run button will be shown and the stop button hidden. If
	/// false, the run button will be hidden and the stop button shown.
	/// </param>
	void ShowRunButton(bool show);

	/// <summary>
	/// Get a reference to the graphs view.
	/// </summary>
	/// <value></value>
	IGraphsView GraphsView { get; }
}
