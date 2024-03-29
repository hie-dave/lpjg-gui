using Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which allows the user to view the raw outputs from the model.
/// </summary>
public class OutputsView : Box
{
	/// <summary>
	/// Create a new <see cref="OutputsView"/> instance.
	/// </summary>
	public OutputsView()
	{
		Label label = Label.New("Outputs: to be implemented");
		label.Halign = Align.Center;
		label.Valign = Align.Center;
		label.Hexpand = true;
		Append(label);
	}
}
