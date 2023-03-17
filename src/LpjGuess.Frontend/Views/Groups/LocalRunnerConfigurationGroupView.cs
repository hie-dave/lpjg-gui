using Adw;
using Gtk;
using LpjGuess.Core.Runners.Configuration;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views.Rows;

namespace LpjGuess.Frontend.Views.Runners;

/// <summary>
/// A view which displays settings for a <see cref="LocalRunnerConfiguration"/>.
/// </summary>
internal class LocalRunnerConfigurationGroupView : IGroupView
{
	/// <summary>
	/// Row title/property description.
	/// </summary>
	private const string title = "Path to the LPJ-Guess executable";

	/// <summary>
	/// The preferences group containing the view's UI controls.
	/// </summary>
	private readonly PreferencesGroup group;

	/// <summary>
	/// Invoked when the path to the guess executable is changed by the user.
	/// </summary>
	public Event<string> OnGuessPathChanged;

	/// <summary>
	/// The row containing guess executable inputs.
	/// </summary>
	private readonly PreferencesRow executableRow;

	/// <summary>
	/// Create a new <see cref="LocalRunnerConfigurationGroupView"/> instance.
	/// </summary>
	/// <param name="guessPath">Path to the guess executable.</param>
	public LocalRunnerConfigurationGroupView(string guessPath)
	{
		OnGuessPathChanged = new Event<string>();

		executableRow = new FileChooserRow(title, guessPath, true);
		group = new PreferencesGroup();
		group.Add(executableRow);
	}

	public void Dispose()
	{
		OnGuessPathChanged.DisconnectAll();
		executableRow.Dispose();
		group.Dispose();
	}

	/// <inheritdoc />
	public PreferencesGroup GetGroup() => group;

	/// <inheritdoc />
	public Widget GetWidget() => GetGroup();
}
